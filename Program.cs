using System;
using System.IO;
using Dicom;
using System.Linq;
using System.Collections.Generic;

namespace Anonymizer
{
    class Program
    {
        private static Random random = new Random();
        private static DicomAnonymizer dicomAnonymizer = new DicomAnonymizer();
        private static List<string> listaNaoDicom = new List<string>();
        static int nFiles; 

        static void Main()
        {
            Console.WriteLine("Insira o endereço do diretório de origem:");
            string input = Console.ReadLine();
            while (!Directory.Exists(input))
            {
                Console.WriteLine("Diretório de origem inválido.");
                Console.WriteLine("Insira o endereço do diretório de origem:");
                input = Console.ReadLine();
            }

            Console.WriteLine("Insira o endereço do diretório de destino:");
            string output = Console.ReadLine();
            while (!Directory.Exists(output))
            {
                Console.WriteLine("Diretório de destino inválido.");
                Console.WriteLine("Insira o endereço do diretório de destino:");
                output = Console.ReadLine();
            }

            Anonymize(input, output);

            Console.Clear();
            if (nFiles-listaNaoDicom.Count != 0)
                Console.WriteLine($"FIM\n{nFiles-listaNaoDicom.Count} arquivos DICOM anonimizados foram salvos no diretório \"{output}\".");
            else
                Console.WriteLine($"FIM\nO programa não identificou nenhum arquivo DICOM no diretório de origem. Nenhum arquivo foi salvo no diretório de destino.");

            if (listaNaoDicom.Count > 0)
            {
                if (listaNaoDicom.Count == 1)
                    Console.WriteLine("\nAviso:");
                else
                    Console.WriteLine("\nAvisos:");
                foreach (string naoDicom in listaNaoDicom)
                    Console.WriteLine($"O arquivo \"{naoDicom.Substring(1)}\" não foi anonimizado porque não tem cabeçalho DICOM válido.");
            }  
        }

        static void Anonymize(string input, string output)
        {
            string[] filePaths = Directory.GetFiles(@input, "*.*", SearchOption.AllDirectories);
            string randomPatientName = RandomString(15);
            int prevProg = 0;
            for (int i = 0; i < filePaths.Length; i++)
            {
                string selPath = filePaths[i].Substring(input.Length);   
                string newFilePath = output + selPath;
                string folderPath = selPath.Substring(0, selPath.LastIndexOf(@"\"));
                CreateDirectories(output, folderPath);
                RemoveTagsFromFile(filePaths[i], newFilePath, randomPatientName);
                int prog = (i*100)/filePaths.Length;
                if (prog != prevProg)
                {
                    Console.Clear();
                    Console.WriteLine($"Progesso: {prog}%");
                    prevProg = prog;
                }
            }
            nFiles = filePaths.Length;   
        }

        static void CreateDirectories(string output, string selPath)
        {
            IEnumerable<int> indexes = AllIndexesOf(selPath, @"\").OrderByDescending(x => x);
            foreach (int index in indexes)
            {
                int length = selPath.Length - index;
                string folder = selPath.Substring(0, length);
                string newFolderPath = output + folder;
                if (!Directory.Exists(newFolderPath))
                        Directory.CreateDirectory(newFolderPath);
            }; 
        }

        static void RemoveTagsFromFile(string filePath, string newFilePath, string randomPatientName)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf(@"\"));
            if (DicomFile.HasValidHeader(filePath))
            {
                var dicomFile = Dicom.DicomFile.Open(filePath);
                if (dicomFile.Dataset.Contains(DicomTag.SOPInstanceUID))
                {
                    DicomFile newFile = dicomAnonymizer.Anonymize(dicomFile);
                    newFile.Dataset.AddOrUpdate<string>(DicomTag.PatientName, randomPatientName);
                    newFile.Dataset.AddOrUpdate<string>(DicomTag.PatientIdentityRemoved, "YES");
                    newFile.Dataset.AddOrUpdate<string>(DicomTag.DeidentificationMethod, "Anonimizado por Fabio Freller - Hospital Alemão Oswaldo Cruz");
                    newFile.Save(Path.Combine(newFilePath));
                }
                else
                {
                    dicomFile.Save(Path.Combine(newFilePath));
                }
            }
            else
            {
                listaNaoDicom.Add(fileName);
                File.Copy(@filePath, @newFilePath);
            }
        }

        public static string RandomString(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static IEnumerable<int> AllIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty: " + value);
            for (int index = 0;; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    break;
                yield return index;
            }       
        }
    }
}