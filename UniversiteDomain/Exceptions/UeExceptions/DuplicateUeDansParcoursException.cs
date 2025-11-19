using System;
using System.Runtime.Serialization;

namespace UniversiteDomain.Exceptions.ParcoursExceptions
{
    [Serializable]
    public class DuplicateUeDansParcoursException : Exception
    {
        public DuplicateUeDansParcoursException()
            : base("Cette UE est déjà associée à ce parcours.") { }

        public DuplicateUeDansParcoursException(string message)
            : base(message) { }

        public DuplicateUeDansParcoursException(long idUe, long idParcours)
            : base($"L’UE {idUe} est déjà présente dans le parcours {idParcours}.") { }

        public DuplicateUeDansParcoursException(string message, Exception inner)
            : base(message, inner) { }

        protected DuplicateUeDansParcoursException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}