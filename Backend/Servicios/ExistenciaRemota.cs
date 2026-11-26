public class ExistenciaRemota
{
    private string CodigoSede;
    private string CodigoProducto;
    private int Disponible;
    private DateTime UltimaActualización;

    public string GetCodigoSede() {return CodigoSede;}
    public string GetCodigoProducto() {return CodigoProducto;}
    public int GetDisponible() {return Disponible;}
    public DateTime GetUltimaActualización() {return UltimaActualización;}

    public void SetDisponible(int disponible) {this.Disponible = disponible;}
    public void SetUltimaActualización(DateTime nuevo) {this.UltimaActualización = nuevo;}
}