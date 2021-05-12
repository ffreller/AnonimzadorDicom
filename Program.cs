using System;
using System.IO;
using System.Collections.Generic;

namespace AnonimizadorDicom
{
    class Program
    {
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

            Anonimizador anonimizador = new Anonimizador();
            anonimizador.AnonymizeDirectory(input, output);
            int nFiles = anonimizador.nFiles;
            List<string> listaNaoDicom = anonimizador.listaNaoDicom;

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
    }
}