namespace UniversiteDomain.Exceptions.UeExceptions;

[Serializable]
public class UeNotFoundException : Exception
{
    public UeNotFoundException()
        : base("L'UE demandee est introuvable.") { }

    public UeNotFoundException(string id)
        : base($"Aucune UE trouvee avec l'id {id}.") { }

    public UeNotFoundException(string message, Exception inner)
        : base(message, inner) { }
}
