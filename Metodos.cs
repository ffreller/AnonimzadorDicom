using System;
using System.IO;
using Dicom;
using System.Linq;
using System.Collections.Generic;

namespace AnonimizadorDicom
{
    public class Metodos
    {
        private static DicomAnonymizer dicomAnonymizer = new DicomAnonymizer();
        public List<string> listaNaoDicom = new List<string>();
        public int nFiles { get; set; }
        public void AnonymizeDirectory(string input, string output)
        {
            string[] filePaths = Directory.GetFiles(@input, "*.*", SearchOption.AllDirectories);
            nFiles =  filePaths.Length;
            // string randomPatientName = RandomString(15);
            int prevProg = 0;
            for (int i = 0; i < filePaths.Length; i++)
            {
                string selPath = filePaths[i].Substring(input.Length);   
                string newFilePath = output + selPath;
                string folderPath = selPath.Substring(0, selPath.LastIndexOf(@"\"));
                CreateDirectories(output, folderPath);
                // AnonymizeFile(filePaths[i], newFilePath, randomPatientName);
                AnonymizeFile(filePaths[i], newFilePath);
                int prog = (i*100)/filePaths.Length;
                if (prog != prevProg)
                {
                    Console.Clear();
                    Console.WriteLine($"Progesso: {prog}%");
                    prevProg = prog;
                }
            }
        }

        private void CreateDirectories(string output, string selPath)
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

        // private void AnonymizeFile(string filePath, string newFilePath, string randomPatientName)
        private void AnonymizeFile(string filePath, string newFilePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf(@"\"));
            if (DicomFile.HasValidHeader(filePath))
            {
                var dicomFile = Dicom.DicomFile.Open(filePath);
                if (dicomFile.Dataset.Contains(DicomTag.SOPInstanceUID))
                {
                    DicomFile newFile = dicomAnonymizer.Anonymize(dicomFile);
                    string sereisUid = newFile.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID);
                    newFile.Dataset.AddOrUpdate<string>(DicomTag.PatientName, sereisUid);
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

        // private string RandomString(int length)
        // {
        //     Random random = new Random();
        //     string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //     return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        // }

        private IEnumerable<int> AllIndexesOf(string str, string value)
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