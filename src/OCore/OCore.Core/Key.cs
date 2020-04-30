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
        Long,
        LongCompound
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
                return new Key { Long = grain.GetPrimaryKeyLong(), Type = KeyType.Long };
            }
            else if (grain is IGrainWithIntegerCompoundKey)
            {
                var @long = grain.GetPrimaryKeyLong(out var keyExtension);
                return new Key
                {
                    Long = @long,
                    Extension = keyExtension,
                    Type = KeyType.LongCompound
                };             
            }
            throw new InvalidOperationException("Unable to create key from grain");
        }

        public override bool Equals(object obj)
        {
            return obj is Key key &&
                   Long == key.Long &&
                   String == key.String &&
                   EqualityComparer<Guid?>.Default.Equals(Guid, key.Guid) &&
                   Extension == key.Extension &&
                   Type == key.Type;
        }

        public override int GetHashCode()
        {
            int hashCode = -1093556722;
            hashCode = hashCode * -1521134295 + Long.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(String);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Extension);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            switch (Type) 
            {
                case KeyType.String:
                    return $"String: {String}";
                case KeyType.Guid:
                    return $"Guid: {Guid}";
                case KeyType.GuidCompound:
                    return $"Guid: {Guid} Extension: {Extension}";
                case KeyType.Long:
                    return $"Long: {Long}";
                case KeyType.LongCompound:
                    return $"Long: {Long} Extension: {Extension}";
            }
            return "Unknown key type";
        }
    }

}
