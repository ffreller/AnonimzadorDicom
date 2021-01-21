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

        private static void CreateDirectories(string output, string selPath)
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

        private static void AnonymizeFile(string filePath, string newFilePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf(@"\"));
            if (DicomFile.HasValidHeader(filePath))
            {
                var dicomFile = Dicom.DicomFile.Open(filePath);
                if (dicomFile.Dataset.Contains(DicomTag.SOPInstanceUID))
                {
                    string patientName = dicomFile.Dataset.GetSingleValue<string>(DicomTag.PatientName).Encrypt();
                    DicomFile newFile = dicomAnonymizer.Anonymize(dicomFile);
                    newFile.Dataset.AddOrUpdate<string>(DicomTag.PatientName, patientName);
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
                if (!File.Exists(@newFilePath))
                    File.Copy(@filePath, @newFilePath);
            }
        }

        private static IEnumerable<int> AllIndexesOf(string str, string value)
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

        private static string Encrypt(this string clearText)
        {
            string EncryptionKey = "AnonimizadorFabioFrellerHAOC";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
    }
}