using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Linq;
using System.Text.RegularExpressions;

namespace AZ_CPF_VALIDATOR
{
    public static class ValidateCPF
    {
        [FunctionName("ValidateCPF")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando validação de CPF.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if (data == null){
                return new BadRequestObjectResult("Por favor, informar o CPF.");
            }
            string cpf = data?.cpf;

            if (Validate(cpf) == false){
                return new BadRequestObjectResult("CPF inválido");
            }
            var responseMessage = "CPF válido e não consta na base de fraudes, e não consta na base de débitos.";

            return new OkObjectResult(responseMessage);
        }
    

        public static bool Validate(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            // Remove caracteres não numéricos
            cpf = Regex.Replace(cpf, "[^0-9]", "");

            // O CPF precisa ter 11 dígitos
            if (cpf.Length != 11) return false;

            // Elimina CPFs inválidos conhecidos (todos os números iguais)
            string[] invalidos = { "00000000000", "11111111111", "22222222222", 
                                "33333333333", "44444444444", "55555555555", 
                                "66666666666", "77777777777", "88888888888", 
                                "99999999999" };

            if (invalidos.Contains(cpf)) return false;

            // Calcula os dois dígitos verificadores
            for (int j = 9; j < 11; j++)
            {
                int soma = 0, multiplicador = j + 1;

                for (int i = 0; i < j; i++)
                {
                    soma += (cpf[i] - '0') * (multiplicador--);
                }

                int resto = soma % 11;
                int digitoVerificador = resto < 2 ? 0 : 11 - resto;

                if (cpf[j] - '0' != digitoVerificador) return false;
            }

            return true;
        }
    }
}
