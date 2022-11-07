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
                    string[] outputCSV = CreateVaccinationOrder(inputCSV, dosesInStock, vaccinateAgeUnder18);
                    if (outputCSV != null)
                    {
                        if (!File.Exists(outputDataPath))
                        {
                            File.Create(outputDataPath);
                            File.WriteAllLines(outputDataPath, outputCSV);
                            Console.WriteLine("Resultatet sparades i " + outputDataPath);
                        }
                        else if (File.Exists(outputDataPath))
                        {
                            int option2 = ShowMenu("Filen finns redan vid angedd sökväg. Vill du skriva över den?", new[]
                            {
                            "Ja",
                            "Nej"
                        });

                            if (option2 == 0)
                            {
                                File.WriteAllLines(outputDataPath, outputCSV);
                                Console.WriteLine("Resultatet sparades i " + outputDataPath);
                            }
                            else if (option2 == 1)
                            {
                                Console.WriteLine("Prioritetsordning sparades inte. Går tillbaka till huvudmeny.");
                            }
                        }
                    }
                    else
                    {
                        Console.ReadLine(); // ReadLine here so that user can see error messages for wrong format in input CSV file
                    }

                    Console.ReadLine();
                    Console.Clear();

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
                            Console.ReadLine();
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Någonting gick fel. Bekräfta att du lägger in en siffra.");
                        Console.ReadLine();
                    }

                    Console.Clear();
                }
                else if (option == 2) // Change age limit
                {
                    int option3 = ShowMenu("Ska personer under 18 vaccineras?", new[]
                    {
                        "Ja",
                        "Nej"
                    });
                    if (option3 == 0)
                    {
                        vaccinateAgeUnder18 = true;
                    }
                    else if (option3 == 1)
                    {
                        vaccinateAgeUnder18 = false;
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
            bool isOk = true;

            foreach (string p in input)
            {
                splitCSVList.Add(p.Split(','));
            }

            foreach (string[] array in splitCSVList)
            {
                array[0] = StandardizeID(array[0]);
            }

            //error handling for incorrect values in input CSV
            foreach (string[] array in splitCSVList)
            {
                try
                {
                    if (array[0].Length != 13 || !array[0].Contains('-') || array[1] == "" || array[2] == "" || array[3] != "0" && array[3] != "1"
                        || array[4] != "0" && array[4] != "1" || array[5] != "0" && array[5] != "1")
                    {
                        throw new Exception("Invalid CSV file format");
                    }
                }
                catch
                {
                    Console.WriteLine("Ogiltigt värde upptäckt i indatafilen.");
                    isOk = false;
                }
            }
            if (!isOk) return null;

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

            var priorityOrder = patientList.OrderByDescending(p => p.HealthCareWorker).ThenByDescending(p => CalculateAge(p.IDNumber) >= 65)
                .ThenByDescending(p => p.RiskGroup).ThenByDescending(p => CalculateAge(p.IDNumber));

            List<string> outputList = new List<string>();
            outputList = ProcessOutputList(priorityOrder);


            string[] outputCSV = ConvertToCSVFormat(outputList, outputList.Count / 4);
            return outputCSV;
        }

        //processes bool to return string that gives a more readable display to user 
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
                Console.WriteLine("Sökväg för indatafil ändrad.");
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
        public static List<string> ProcessOutputList(IOrderedEnumerable<Patient> priorityOrder)
        {
            List<string> outputList = new List<string>();
            foreach (Patient p in priorityOrder)
            {
                if (vaccinateAgeUnder18 == true)
                {
                    outputList.Add(p.IDNumber);
                    outputList.Add(p.LastName);
                    outputList.Add(p.FirstName);
                    outputList.Add(CalculateDosesForPatient(p.PreviouslyInfected));
                }
                else if (vaccinateAgeUnder18 == false)
                {
                    if (CalculateAge(p.IDNumber) >= 18)
                    {
                        outputList.Add(p.IDNumber);
                        outputList.Add(p.LastName);
                        outputList.Add(p.FirstName);
                        outputList.Add(CalculateDosesForPatient(p.PreviouslyInfected));
                    }
                }
            }
            return outputList;
        }

        //determines how many doses of vaccine each patient should receive in the ProcessOutPutList method
        //also checks if there are enough vaccine doses in stock before assigning how many doses to give to a patient
        //if there aren't enough doses patient is assigned 0 doses
        //used to cut down on otherwise repetitive code
        public static string CalculateDosesForPatient(int wasInfected)
        {
            string dosesForPatient = "";
            if (wasInfected == 0)
            {
                dosesForPatient = "2";
                if (dosesInStock - int.Parse(dosesForPatient) < 0)
                {
                    dosesForPatient = "0";
                }
                else
                {
                    Program.dosesInStock -= int.Parse(dosesForPatient);
                }
            }
            else if (wasInfected == 1)
            {
                dosesForPatient = "1";
                if (dosesInStock - int.Parse(dosesForPatient) < 0)
                {
                    dosesForPatient = "0";
                }
                else
                {
                    Program.dosesInStock -= int.Parse(dosesForPatient);
                }
            }
            return dosesForPatient;
        }

        //converts list into a string array in the CSV format
        //second parameter determines the array length
        //the method creates a string from the fist 4 indexes of the parameter list and adds it to the string array outputCSV
        //it then repeats the process for the next 4 indexes of the parameter list etc
        //the length of array outputCSV will therefore be 1/4th as long as the Count of outputList
        public static string[] ConvertToCSVFormat(List<string> outputList, int arrayLength)
        {
            string[] outputCSV = new string[arrayLength];

            //keeps track of the current index of outputList to add to the patient string
            int indexCounter = 0;

            for (int i = 0; i < arrayLength; i++)
            {
                string patient = "";
                for (int j = 0; j < 4; j++)
                {
                    patient = patient + outputList[indexCounter] + ",";

                    indexCounter++;
                }
                string lastCommaRemoved = patient.Remove(patient.Length - 1, 1);
                outputCSV[i] = lastCommaRemoved;
            }

            return outputCSV;
        }

        public static string StandardizeID(string idNumber)
        {
            try
            {
                string shortenedID = idNumber.Replace("-", "");

                if (idNumber.Length == 10)
                {
                    shortenedID = "19" + shortenedID;
                }
                string endOfID = shortenedID.Substring(8, 4);
                shortenedID = shortenedID.Replace(endOfID, "");
                idNumber = shortenedID + "-" + endOfID;
                return idNumber;
            }
            catch
            {
                return idNumber;
            }

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
            Program.dosesInStock = 10;
            Program.vaccinateAgeUnder18 = false;

            // Act
            string[] output = Program.CreateVaccinationOrder(input, Program.dosesInStock, Program.vaccinateAgeUnder18);

            // Assert
            Assert.AreEqual(output.Length, 2);
            Assert.AreEqual("19810203-2222,Efternamnsson,Eva,2", output[0]);
            Assert.AreEqual("19720906-1111,Elba,Idris,1", output[1]);
        }
        [TestMethod]
        public void MixedAgesChildVaccinationFalse()
        {
            string[] input =
            {
                "20140518-1112,Mortensson,Matilda,0,0,0",
                "19520812-1111,Högfäldt,Ulf,0,0,1",
                "19920103-1113,Annasson,Anna,0,1,0"
            };
            Program.dosesInStock = 10;
            Program.vaccinateAgeUnder18 = false;

            string[] output = Program.CreateVaccinationOrder(input, Program.dosesInStock, Program.vaccinateAgeUnder18);

            Assert.AreEqual(output.Length, 2);
            Assert.AreEqual("19520812-1111,Högfäldt,Ulf,1", output[0]);
            Assert.AreEqual("19920103-1113,Annasson,Anna,2", output[1]);
        }
        [TestMethod]
        public void MixedAgesChildVaccinationTrue()
        {
            string[] input =
            {
                "20140518-1112,Mortensson,Matilda,0,0,0",
                "19520812-1111,Högfäldt,Ulf,0,0,1",
                "19920103-1113,Annasson,Anna,0,1,0"
            };
            Program.dosesInStock = 10;
            Program.vaccinateAgeUnder18 = true;

            string[] output = Program.CreateVaccinationOrder(input, Program.dosesInStock, Program.vaccinateAgeUnder18);

            Assert.AreEqual(output.Length, 3);
            Assert.AreEqual("19520812-1111,Högfäldt,Ulf,1", output[0]);
            Assert.AreEqual("19920103-1113,Annasson,Anna,2", output[1]);
            Assert.AreEqual("20140518-1112,Mortensson,Matilda,2", output[2]);
        }
        [TestMethod]
        public void VariedIDNumberInput()
        {
            string[] input =
            {
                "201405181112,Mortensson,Matilda,0,0,0",
                "5208121111,Högfäldt,Ulf,0,0,1",
                "19920103-1113,Annasson,Anna,0,1,0"
            };
            Program.dosesInStock = 10;
            Program.vaccinateAgeUnder18 = true;

            string[] output = Program.CreateVaccinationOrder(input, Program.dosesInStock, Program.vaccinateAgeUnder18);

            Assert.AreEqual(output.Length, 3);
            Assert.AreEqual("19520812-1111,Högfäldt,Ulf,1", output[0]);
            Assert.AreEqual("19920103-1113,Annasson,Anna,2", output[1]);
            Assert.AreEqual("20140518-1112,Mortensson,Matilda,2", output[2]);
        }
        [TestMethod]
        public void ChildInRiskGroup()
        {
            string[] input =
            {
                "201405181112,Mortensson,Matilda,0,1,0",
                "5208121111,Högfäldt,Ulf,0,0,1",
                "19920103-1113,Annasson,Anna,0,0,0"
            };
            Program.dosesInStock = 10;
            Program.vaccinateAgeUnder18 = true;

            string[] output = Program.CreateVaccinationOrder(input, Program.dosesInStock, Program.vaccinateAgeUnder18);

            Assert.AreEqual(output.Length, 3);
            Assert.AreEqual("19520812-1111,Högfäldt,Ulf,1", output[0]);
            Assert.AreEqual("20140518-1112,Mortensson,Matilda,2", output[1]);
            Assert.AreEqual("19920103-1113,Annasson,Anna,2", output[2]);
        }
        [TestMethod]
        public void NotEnoughVaccines()
        {
            string[] input =
            {
                "201405181112,Mortensson,Matilda,0,1,0",
                "5208121111,Högfäldt,Ulf,0,0,0",
                "19920103-1113,Annasson,Anna,0,0,0"
            };
            Program.dosesInStock = 5;
            Program.vaccinateAgeUnder18 = true;

            string[] output = Program.CreateVaccinationOrder(input, Program.dosesInStock, Program.vaccinateAgeUnder18);

            Assert.AreEqual(output.Length, 3);
            Assert.AreEqual("19520812-1111,Högfäldt,Ulf,2", output[0]);
            Assert.AreEqual("20140518-1112,Mortensson,Matilda,2", output[1]);
            Assert.AreEqual("19920103-1113,Annasson,Anna,0", output[2]);
        }
    }
}
