using System;
using System.IO;
using Dicom;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace AnonimizadorDicom
{
    public static class Metodos
    {
        private static DicomAnonymizer dicomAnonymizer = new DicomAnonymizer();
        public static List<string> listaNaoDicom = new List<string>();
        public static int nFiles { get; set; }
        public static void AnonymizeDirectory(string input, string output)
        {
            string[] filePaths = Directory.GetFiles(@input, "*.*", SearchOption.AllDirectories);
            nFiles =  filePaths.Length;
            int previousProgress = 0;
            
            string[] dirs = Directory.GetDirectories(@input, "*", SearchOption.AllDirectories);
            foreach (string dir in dirs)
            {
                string newFolderPath = output + dir.Substring(input.Length);
                if (!Directory.Exists(newFolderPath))
                    Directory.CreateDirectory(newFolderPath);
            }

            for (int i = 0; i < nFiles; i++)
            {
                string selPath = filePaths[i].Substring(input.Length);   
                string newFilePath = output + selPath;
                string folderPath = selPath.Substring(0, selPath.LastIndexOf(@"\"));
                // if (prog > 61 & prog < 64)
                // {
                //     Console.WriteLine(filePaths[i]);
                //     Console.WriteLine(newFilePath);
                //     Console.WriteLine("----------");
                // }
                AnonymizeFile(filePaths[i], newFilePath);
                int prog = (i*100)/nFiles;
                if (prog != previousProgress)
                {
                    Console.Clear();
                    Console.WriteLine($"Progesso: {prog}%");
                    previousProgress = prog;
                }
            }
        }

        private static void AnonymizeFile(string filePath, string newFilePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf(@"\"));
            if (DicomFile.HasValidHeader(filePath))
            {
                var dicomFile = Dicom.DicomFile.Open(filePath);
                if (dicomFile.Dataset.Contains(DicomTag.SOPInstanceUID))
                {
                    DicomFile newFile = dicomAnonymizer.Anonymize(dicomFile);
                    // string patientName = dicomFile.Dataset.GetValue<string>(DicomTag.PatientName).Encrypt().Substring(0, 30);
                    // newFile.Dataset.AddOrUpdate<string>(DicomTag.PatientName, patientName);
                    // newFile.Dataset.AddOrUpdate<string>(DicomTag.PatientIdentityRemoved, "YES");
                    // newFile.Dataset.AddOrUpdate<string>(DicomTag.DeidentificationMethod, "Anonimizado por Fabio Freller - Hospital Alemão Oswaldo Cruz");
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
                if (!File.Exists(@newFilePath))
                    File.Copy(@filePath, @newFilePath);
            }
        }
    }
}