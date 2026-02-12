using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFCiclo.Data.Models
{
    [Table("user_roles")]
    public class user_roles
    {
        #region Métodos privados
        private int _user_id = 0;
        private int _role_id = 0;

        [Key, Column(Order = 0)]
        public int user_id { get => _user_id; set => _user_id = value; }
        [Key, Column(Order = 0)]
        public int role_id { get => _role_id; set => _role_id = value; }
        #endregion
    }
}
