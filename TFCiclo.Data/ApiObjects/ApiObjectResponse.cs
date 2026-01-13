
namespace TFCiclo.Data.ApiObjects
{
    public class ApiObjectResponse
    {
        #region Métodos públicos
        public bool result { get; set; }

        public object data { get; set; }

        public int error_code { get; set; }

        public string error_message { get; set; }

        #endregion

        #region Constructores
        public ApiObjectResponse() { }

        public ApiObjectResponse(bool result, object data, int error_code, string error_message)
        {
            this.result = result;
            this.data = data;
            this.error_code = error_code;
            this.error_message = error_message;
        }
        #endregion
    }

    #region TokensResponse
    public class TokensResponse
    {


        public string accessToken { get; set; }
        public string refreshToken { get; set; }

        public TokensResponse() { }

        public TokensResponse(string accessToken, string refreshToken)
        {
            this.accessToken = accessToken;
            this.refreshToken = refreshToken;
        }

    }

    #endregion
}
