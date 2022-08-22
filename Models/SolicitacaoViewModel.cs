namespace MvcSite.Models;

public class SolicitacaoViewModel
{
    public Int64 idSolicitacao { get; set; }
    public DateTime dataSolicitacao { get; set; }
    public String descricaoSolicitacao { get; set; } = "";
    public StatusViewModel status { get; set; } = new StatusViewModel();

    public short idStatus { get; set; }
}
