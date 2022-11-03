using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.IO;
using System.Text;

namespace Vaccination
{
    public class Patient
    {
        public string IDNumber;
        public string FirstName;
        public string LastName;
        public int HealthCareWorker;
        public int RiskGroup;
        public int PreviouslyInfected;

        public void StandardizeID() 
        {
            if (!IDNumber.StartsWith("19") && IDNumber.Length <= 13)
            {
                IDNumber = "19" + IDNumber;
            }
            string shortenedID = IDNumber.Replace("-", "");
            
            string endOfID = shortenedID.Substring(8, 4);
            shortenedID = shortenedID.Replace(endOfID, ""); 
            IDNumber = shortenedID + "-" + endOfID;
        }
        
    }
    public class Program
    {
        public static int dosesInStock = 0;
        public static bool vaccinateAgeUnder18 = false;
        public static string inputDataPath = @"C:\Windows\Temp\PatientInfo.csv";
        public static string outputDataPath = @"C:\Windows\Temp\VaccinationList.csv";

        public static void Main()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            
            Console.WriteLine("Välkommen!");
            Console.WriteLine();

            bool running = true;
            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("Antal tillgänliga vaccindoser: " + dosesInStock);
                Console.WriteLine("Vaccinering under 18 år: " + DisplayVaccinationAgeOption(vaccinateAgeUnder18));
                Console.WriteLine("Indatafil: " + inputDataPath);
                Console.WriteLine("Utdatafil: " + outputDataPath);
                Console.WriteLine();

                int option = ShowMenu("Vad vill du göra?", new[]
                {
                    "Skapa prioritetsordning",
                    "Ändra antal vaccindoser",
                    "Ändra åldersgräns",
                    "Ändra indatafil",
                    "Ändra utdatafil",
                    "Avsluta"
                });
                Console.Clear();

                if (option == 0) // Create prioritization list
                {
                    string[] inputCSV = File.ReadAllLines(inputDataPath);
                    CreateVaccinationOrder(inputCSV, dosesInStock, vaccinateAgeUnder18);
                }
                else if (option == 1) // Change number of doses
                {
                    Console.WriteLine("Antal tillgänliga vaccindoser: " + dosesInStock);
                    Console.WriteLine();
                    Console.WriteLine("Ange nytt antal doser: ");
                    
                    try
                    {
                        int newDoses = int.Parse(Console.ReadLine());
                        if (dosesInStock + newDoses >= 0)
                        {
                            dosesInStock = newDoses;
                        }
                        else
                        {
                            Console.WriteLine("Antalet tillgängliga vaccindoser kan inte vara negativa.");
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Någonting gick fel. Bekräfta att du lägger in en siffra.");
                    }
                    
                    Console.ReadLine();
                    Console.Clear();
                }
                else if (option == 2) // Change age limit
                {
                    int option2 = ShowMenu("Ska personer under 18 vaccineras?", new[]
                    {
                        "Ja",
                        "Nej"
                    });
                    if (option2 == 0)
                    {
                        vaccinateAgeUnder18 = true;
                    }
                    else if (option2 == 1)
                    {
                        vaccinateAgeUnder18 = false;
                    }
                    else
                    {
                        Console.WriteLine("Någonting gick fel, var god välj giltigt menyval");
                    }
                }
                else if (option == 3) // Change input data file path
                {
                    Console.WriteLine("Indatafil: " + inputDataPath);
                    Console.WriteLine();
                    Console.WriteLine("Ange ny sökväg: ");
                    ChangeInputDataPath();
                }
                else if (option == 4) // Change output data file path
                {
                    Console.WriteLine("Utdatafil: " + outputDataPath);
                    Console.WriteLine();
                    Console.WriteLine("Ange ny sökväg: ");
                    ChangeOutputDataPath();
                    
                }
                else if (option == 5) // End program
                {
                    running = false;
                }
                else
                {
                    Console.WriteLine("Någonting gick fel, var god välj giltigt menyval");
                }
            }
        }

        // Create the lines that should be saved to a CSV file after creating the vaccination order.
        //
        // Parameters:
        //
        // input: the lines from a CSV file containing population information
        // doses: the number of vaccine doses available
        // vaccinateChildren: whether to vaccinate people younger than 18
        
        public static string[] CreateVaccinationOrder(string[] input, int doses, bool vaccinateChildren)
        {
            List<string[]> splitCSVList = new List<string[]>();
            List<Patient> patientList = new List<Patient>();

            foreach (string p in input)
            {
                splitCSVList.Add(p.Split(','));
            }

            for (int i = 0; i < splitCSVList.Count; i++)
            {
                Patient patient = new Patient
                {
                    IDNumber = splitCSVList[i][0],
                    LastName = splitCSVList[i][1],
                    FirstName = splitCSVList[i][2],
                    HealthCareWorker = int.Parse(splitCSVList[i][3]),
                    RiskGroup = int.Parse(splitCSVList[i][4]),
                    PreviouslyInfected = int.Parse(splitCSVList[i][5])

                };
                
                patientList.Add(patient);
            }

            foreach (Patient p in patientList)
            {
                p.StandardizeID();
            }

            var priorityOrder = patientList.OrderByDescending(p => p.HealthCareWorker).ThenBy(p => CalculateAge(p.IDNumber) >= 65).
                ThenByDescending(p => CalculateAge(p.IDNumber)).ThenByDescending(p => p.RiskGroup);

            List<string> outputList = new List<string>();
            outputList = ProcessOutputList(priorityOrder);

            return new string[0];
        }

        //processes bool to return string that gives a more comprehensible display to user 
        public static string DisplayVaccinationAgeOption(bool vaccinateUnder18)
        {
            string ageYesNo = "";
            if (vaccinateUnder18 == true)
            {
                ageYesNo = "Ja";
            }
            else
            {
                ageYesNo = "Nej";
            }
            return ageYesNo;
        }

        //allows user to change the search path for the input data file
        public static void ChangeInputDataPath()
        {

            string newInputDataPath = Console.ReadLine();
            if (File.Exists(newInputDataPath))
            {
                inputDataPath = newInputDataPath;
            }
            else
            {
                Console.WriteLine("Kunde inte hitta filen vid angedd sökväg");
            }
            Console.ReadLine();
            Console.Clear();
        }

        //allows user to change the search path for the output data file
        public static void ChangeOutputDataPath()
        {
            string newOutputPath = Console.ReadLine();
            if (Directory.Exists(Path.GetDirectoryName(newOutputPath)))
            {
                outputDataPath = newOutputPath;
            }
            else
            {
                Console.WriteLine("Kunde inte hitta mappen vid angedd sökväg");
            }


            if (!File.Exists(newOutputPath))
            {
                File.Create(newOutputPath);
            }
            else if (File.Exists(newOutputPath))
            {
                
                int option = ShowMenu("Filen finns redan vid angedd sökväg. Vill du skriva över den?", new[]
                {
                   "Ja",
                   "Nej"
                });

                if (option == 0)
                {
                    File.Create(newOutputPath);
                }
                else if (option == 1)
                {
                    Console.WriteLine("Sökväg sparades inte. Går tillbaka till huvudmeny.");
                }
            }
            Console.ReadLine();
            Console.Clear();
        }

        //takes id number of a person and calculates how old they are based on the current year
        public static int CalculateAge(string idNumber)
        {
            int age = 0;
            int yearOfBirth = int.Parse(idNumber.Substring(0, 4));
            DateTime now = DateTime.Today;
            int currentYear = now.Year;
            age = currentYear - yearOfBirth;

            return age;
        }

        //takes the sorted list of Patients and proccesses it to desired output format (IDNumber, LastName, FirstName, and number of doses per patient)
        public static List <string> ProcessOutputList (IOrderedEnumerable <Patient> priorityOrder)
        {
            List<string> outputList = new List<string>();
            string dosesForPatient = "";
            foreach (Patient p in priorityOrder)
            {
                if (vaccinateAgeUnder18 == true)
                {
                    outputList.Add(p.IDNumber);
                    outputList.Add(p.LastName);
                    outputList.Add(p.FirstName);
                    if (p.PreviouslyInfected == 0)
                    {
                        dosesForPatient = "2";
                        if (dosesInStock - int.Parse(dosesForPatient) < 0)
                        {
                            dosesForPatient = "0";
                            outputList.Add(dosesForPatient);
                        }
                        else
                        {
                            outputList.Add(dosesForPatient);
                            dosesInStock -= int.Parse(dosesForPatient);
                        }
                    }
                    else if (p.PreviouslyInfected == 1)
                    {
                        dosesForPatient = "1";
                        if (dosesInStock - int.Parse(dosesForPatient) < 0)
                        {
                            dosesForPatient = "0";
                            outputList.Add(dosesForPatient);
                        }
                        else
                        {
                            outputList.Add(dosesForPatient);
                            dosesInStock -= int.Parse(dosesForPatient);
                        };
                    }
                }
                else if (vaccinateAgeUnder18 == false)
                {
                    if (CalculateAge(p.IDNumber) >= 18)
                    {
                        outputList.Add(p.IDNumber);
                        outputList.Add(p.LastName);
                        outputList.Add(p.FirstName);
                        if (p.PreviouslyInfected == 0)
                        {
                            dosesForPatient = "2";
                            if (dosesInStock - int.Parse(dosesForPatient) < 0)
                            {
                                dosesForPatient = "0";
                                outputList.Add(dosesForPatient);
                            }
                            else
                            {
                                outputList.Add(dosesForPatient);
                                dosesInStock -= int.Parse(dosesForPatient);
                            };
                        }
                        else if (p.PreviouslyInfected == 1)
                        {
                            dosesForPatient = "1";
                            if (dosesInStock - int.Parse(dosesForPatient) < 0)
                            {
                                dosesForPatient = "0";
                                outputList.Add(dosesForPatient);
                            }
                            else
                            {
                                outputList.Add(dosesForPatient);
                                dosesInStock -= int.Parse(dosesForPatient);
                            };
                        }
                    }
                }
            }

            return outputList;
        }

        public static int ShowMenu(string prompt, IEnumerable<string> options)
        {
            if (options == null || options.Count() == 0)
            {
                throw new ArgumentException("Cannot show a menu for an empty list of options.");
            }

            Console.WriteLine(prompt);

            // Hide the cursor that will blink after calling ReadKey.
            Console.CursorVisible = false;

            // Calculate the width of the widest option so we can make them all the same width later.
            int width = options.Max(option => option.Length);

            int selected = 0;
            int top = Console.CursorTop;
            for (int i = 0; i < options.Count(); i++)
            {
                // Start by highlighting the first option.
                if (i == 0)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.White;
                }

                var option = options.ElementAt(i);
                // Pad every option to make them the same width, so the highlight is equally wide everywhere.
                Console.WriteLine("- " + option.PadRight(width));

                Console.ResetColor();
            }
            Console.CursorLeft = 0;
            Console.CursorTop = top - 1;

            ConsoleKey? key = null;
            while (key != ConsoleKey.Enter)
            {
                key = Console.ReadKey(intercept: true).Key;

                // First restore the previously selected option so it's not highlighted anymore.
                Console.CursorTop = top + selected;
                string oldOption = options.ElementAt(selected);
                Console.Write("- " + oldOption.PadRight(width));
                Console.CursorLeft = 0;
                Console.ResetColor();

                // Then find the new selected option.
                if (key == ConsoleKey.DownArrow)
                {
                    selected = Math.Min(selected + 1, options.Count() - 1);
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    selected = Math.Max(selected - 1, 0);
                }

                // Finally highlight the new selected option.
                Console.CursorTop = top + selected;
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                string newOption = options.ElementAt(selected);
                Console.Write("- " + newOption.PadRight(width));
                Console.CursorLeft = 0;
                // Place the cursor one step above the new selected option so that we can scroll and also see the option above.
                Console.CursorTop = top + selected - 1;
                Console.ResetColor();
            }

            // Afterwards, place the cursor below the menu so we can see whatever comes next.
            Console.CursorTop = top + options.Count();

            // Show the cursor again and return the selected option.
            Console.CursorVisible = true;
            return selected;
        }
    }

    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void ExampleTest()
        {
            // Arrange
            string[] input =
            {
                "19720906-1111,Elba,Idris,0,0,1",
                "8102032222,Efternamnsson,Eva,1,1,0"
            };
            int doses = 10;
            bool vaccinateChildren = false;

            // Act
            string[] output = Program.CreateVaccinationOrder(input, doses, vaccinateChildren);

            // Assert
            Assert.AreEqual(output.Length, 2);
            Assert.AreEqual("19810203-2222,Efternamnsson,Eva,2", output[0]);
            Assert.AreEqual("19720906-1111,Elba,Idris,1", output[1]);
        }
    }
}
