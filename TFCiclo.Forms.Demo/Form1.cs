using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TFCiclo.Data.ApiObjects;

namespace TFCiclo.Forms.Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnVerTiempo_Click_1(object sender, EventArgs e)
        {
            // Deshabilitar el botón para evitar reentradas y mostrar feedback
            btnVerTiempo.Enabled = false;
            lblEstadoVertiempo.Text = "Cargando...";

            try
            {
                //Llamo por https y guardo la respuesta
                forecast_weatherResponse respuestaObj = await EnviarObjetoHttpsAsync();

                //Me aseguro que no sea null
                if (respuestaObj == null)
                {
                    lblEstadoVertiempo.Text = "Sin datos";
                    return;
                }

                //Serializo la respuesta
                string textoData = System.Text.Json.JsonSerializer.Serialize(
                    respuestaObj,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

                // Mostrar la respuesta en el TextBox
                tbxResultadoVertiempo.Text = textoData;
            }
            catch (Exception ex)
            {
                //Error
                lbxConsola.Items.Add($"Error al obtener los datos: {ex.Message}");
                lblEstadoVertiempo.Text = "Error";
            }
            finally
            {
                //Fin proceso
                lbxConsola.Items.Add("— Proceso finalizado —");
                btnVerTiempo.Enabled = true;
            }
        }

        #region Métodos privados
        // Envía un objeto como JSON por HTTPS y deserializa la respuesta JSON del data.
        private async Task<forecast_weatherResponse> EnviarObjetoHttpsAsync()
        {
            try
            {
                // URL del endpoint HTTPS
                string url = "https://localhost:7008/api/weather";

                // Crear el objeto a enviar
                ApiLogin solicitud = new ApiLogin
                {
                    Username = "gonzalo",
                    Role = "user",
                    AccesToken = "token1234",
                };

                // Serializar el objeto a JSON
                string jsonSolicitud = JsonSerializer.Serialize(solicitud);
                lbxConsola.Items.Add($"Enviando solicitud a {url}...");

                // Preparar el contenido HTTP (JSON con UTF-8)
                StringContent contenido = new StringContent(jsonSolicitud, Encoding.UTF8, "application/json");

                // Crear cliente HTTP (puedes moverlo a un campo si lo reutilizas)
                using (HttpClient cliente = new HttpClient())
                {
                    // Enviar la solicitud POST
                    HttpResponseMessage respuesta = await cliente.PostAsync(url, contenido);

                    // Validar que la respuesta fue exitosa (200–299)
                    respuesta.EnsureSuccessStatusCode();

                    // Leer la respuesta JSON como string
                    string jsonRespuesta = await respuesta.Content.ReadAsStringAsync();
                    lbxConsola.Items.Add("Respuesta HTTP recibida correctamente.");

                    // Deserializar al tipo esperado (usa tu clase de respuesta)
                    ApiObjectResponse datos = JsonSerializer.Deserialize<ApiObjectResponse>(jsonRespuesta);

                    // 'datos.data' es object, que viene como JsonElement
                    JsonElement elemento = (JsonElement)datos.data;

                    // Deserializar al tipo concreto
                    forecast_weatherResponse result = elemento.Deserialize<forecast_weatherResponse>();

                    //Devolver datos
                    return result;
                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error en la solicitud HTTPS:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lbxConsola.Items.Add($"Error en la solicitud HTTPS:\n{ex.Message}");
                return null;
            }
        }

        #endregion


    }
}
