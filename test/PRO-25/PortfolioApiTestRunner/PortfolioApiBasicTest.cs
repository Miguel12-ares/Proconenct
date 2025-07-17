// using System;
// using System.IO;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Threading.Tasks;

// namespace PortfolioApiTestRunner
// {
//     public class PortfolioApiBasicTest
//     {
//         private readonly string _baseUrl = "http://localhost:5089/api/professionals/portfolio";
//         private readonly string _jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI2ODc1MWMxYzIzMzBmOTk3ODg0OGIxMTkiLCJlbWFpbCI6Im1pZ3VlbDEyLmFkc29AZ21haWwuY29tIiwiZ2l2ZW5fbmFtZSI6Ik1pZ3VlbCBTdGl2ZW4yIiwiZmFtaWx5X25hbWUiOiJDb3J0ZXMgRHVhcnRlMiIsInVuaXF1ZV9uYW1lIjoiTWlndWVsIFN0aXZlbjIgQ29ydGVzIER1YXJ0ZTIiLCJyb2xlIjoiUHJvZmVzc2lvbmFsIiwidXNlcl90eXBlIjoiUHJvZmVzc2lvbmFsIiwiaWQiOiI2ODc1MWMxYzIzMzBmOTk3ODg0OGIxMTkiLCJqdGkiOiJiM2E3NDg5Ni0yYTNlLTQ4ZDMtOGNkMS0xZjJkNWVhYjRlNmMiLCJpYXQiOjE3NTI1NDY2ODAsIm5iZiI6MTc1MjU0NjY4MCwiZXhwIjoxNzUyNTUwMjgwLCJpc3MiOiJQcm9Db25uZWN0LkFQSSIsImF1ZCI6IlByb0Nvbm5lY3QuQ2xpZW50In0.k1Fat4qAG9fwriP3DCXl3lyracEMogdOt-rZdNHBxRA"; // Reemplaza por un token válido

//         public async Task RunAllAsync()
//         {
//             Console.WriteLine("== Prueba subida de archivo ==");
//             var fileId = await TestUploadAsync();
//             Console.WriteLine($"Archivo subido con ID: {fileId}");

//             Console.WriteLine("== Prueba listado de archivos ==");
//             await TestListAsync();

//             if (!string.IsNullOrEmpty(fileId))
//             {
//                 Console.WriteLine("== Prueba eliminación de archivo ==");
//                 await TestDeleteAsync(fileId);
//             }
//         }

//         public async Task<string?> TestUploadAsync()
//         {
//             using var client = new HttpClient();
//             client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

//             var filePath = "testfile.pdf"; // Cambia por un archivo real
//             if (!File.Exists(filePath))
//             {
//                 Console.WriteLine($"Archivo de prueba no encontrado: {filePath}");
//                 return null;
//             }

//             using var form = new MultipartFormDataContent();
//             using var fileStream = File.OpenRead(filePath);
//             var fileContent = new StreamContent(fileStream);
//             fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
//             form.Add(fileContent, "file", Path.GetFileName(filePath));
//             form.Add(new StringContent("Archivo de prueba para portafolio"), "description");

//             var response = await client.PostAsync(_baseUrl + "/upload", form);
//             var result = await response.Content.ReadAsStringAsync();
//             Console.WriteLine($"Respuesta subida: {result}");
//             if (response.IsSuccessStatusCode && result.Contains("id"))
//             {
//                 var id = System.Text.Json.JsonDocument.Parse(result).RootElement.GetProperty("id").GetString();
//                 return id;
//             }
//             return null;
//         }

//         public async Task TestListAsync()
//         {
//             using var client = new HttpClient();
//             client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
//             var response = await client.GetAsync(_baseUrl);
//             var result = await response.Content.ReadAsStringAsync();
//             Console.WriteLine($"Respuesta listado: {result}");
//         }

//         public async Task TestDeleteAsync(string fileId)
//         {
//             using var client = new HttpClient();
//             client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
//             var response = await client.DeleteAsync(_baseUrl + $"/{fileId}");
//             var result = await response.Content.ReadAsStringAsync();
//             Console.WriteLine($"Respuesta eliminación: {result}");
//         }
//     }
// } 