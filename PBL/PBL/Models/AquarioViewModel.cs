namespace PBL.Models
{
    public class AquarioViewModel : PadraoViewModel
    {
        public string Nome { get; set; }
        public decimal CapacidadeLitros { get; set; }
        public string TipoAgua { get; set; }
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }
    }
}
