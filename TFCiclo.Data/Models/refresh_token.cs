using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TFCiclo.Data.Models
{
    namespace TFCiclo.Data.Models
    {
        public class refresh_token
        {
            private int _id = 0;
            private int _userId = 0;
            private string _tokenHash = string.Empty;
            private DateTime _createdAt = DateTime.MinValue;
            private DateTime _expiresAt = DateTime.MinValue;
            private DateTime _revokedAt = DateTime.MinValue;
            private string _replacedByTokenHash = string.Empty;
            private user_info? _user = null;

            [Key]
            public int id { get => _id; set => _id = value; }
            [Required]
            public int userId { get => _userId; set => _userId = value; }
            [Required]
            [MaxLength(512)]
            public string tokenHash { get => _tokenHash; set => _tokenHash = value; }
            public DateTime createdAt { get => _createdAt; set => _createdAt = value; }
            public DateTime expiresAt { get => _expiresAt; set => _expiresAt = value; }
            public DateTime revokedAt { get => _revokedAt; set => _revokedAt = value; }
            public string replacedByTokenHash { get => _replacedByTokenHash; set => _replacedByTokenHash = value; }

            // Navigation
            [ForeignKey("userId")]
            public user_info? user { get => _user; set => _user = value; }
        }
    }
}
