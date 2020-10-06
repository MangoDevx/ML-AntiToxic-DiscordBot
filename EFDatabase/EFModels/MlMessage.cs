using System.ComponentModel.DataAnnotations;

namespace MLNetDBot.EFDatabase.EFModels
{
    public class MlMessage
    {
        [Key] public int Id { get; set; }

        public bool Toxic { get; set; }
        public string Message { get; set; }
    }
}
