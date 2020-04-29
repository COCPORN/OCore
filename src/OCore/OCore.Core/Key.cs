using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Core
{
    public enum KeyType
    {
        String,
        GuidCompound,
        Guid,
        Integer,
        IntegerCompound
    }

    public class Key
    {
        public long? Long { get; set; }

        public string String { get; set; }

        public Guid? Guid { get; set; }

        public string Extension { get; set; }

        public KeyType Type { get; set; }

        public static Key FromGrain(IAddressable grain)
        {
            if (grain is IGrainWithStringKey)
            {                            
                return new Key { String = grain.GetPrimaryKeyString(), Type = KeyType.String };
            }
            else if (grain is IGrainWithGuidCompoundKey)
            {
                var guid = grain.GetPrimaryKey(out var keyExtension);
                return new Key
                {
                    Guid = guid,
                    Extension = keyExtension,
                    Type = KeyType.GuidCompound
                };
            }
            else if (grain is IGrainWithGuidKey)
            {
                return new Key { Guid = grain.GetPrimaryKey(), Type = KeyType.Guid };
            }
            else if (grain is IGrainWithIntegerKey)
            {
                return new Key { Long = grain.GetPrimaryKeyLong(), Type = KeyType.Integer };
            }
            else if (grain is IGrainWithIntegerCompoundKey)
            {
                var @long = grain.GetPrimaryKeyLong(out var keyExtension);
                return new Key
                {
                    Long = @long,
                    Extension = keyExtension,
                    Type = KeyType.IntegerCompound
                };             
            }
            throw new InvalidOperationException("Unable to create key from grain");
        }

        string stringRepresentation;
        public override string ToString()
        {
            switch (Type) 
            {
                case KeyType.String:
                    return String;
                case KeyType.Guid:
                    return Guid.ToString();
                case KeyType.GuidCompound:
                    return $"{Guid}:{Extension}";
                case KeyType.Integer:
                    return $"{Long}";
                case KeyType.IntegerCompound:
                    return $"{Long}:{Extension}";
            }
            return "Unknown key type";
        }
    }

}
