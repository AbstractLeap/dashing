namespace Dashing.Engine.Dialects {
    public class ColumnSpecification {
        /// <summary>
        /// The string that is specified as the type name when creating or modifying the column
        /// </summary>
        /// <remarks>May be different to the DbType e.g. DbType.Guid stored as char in MySql</remarks>
        public string DbTypeName { get; set; }

        /// <summary>
        /// The length of the column that should be used
        /// </summary>
        /// <remarks>A value of -1 indicates that max (or equiv) should be used</remarks>
        public int? Length { get; set; }

        public byte? Precision { get; set; }

        public byte? Scale { get; set; }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var otherSpec = obj as ColumnSpecification;
            if (otherSpec == null) {
                return false;
            }

            return this.DbTypeName == otherSpec.DbTypeName && this.Length.Equals(otherSpec.Length) && this.Precision.Equals(otherSpec.Precision)
                   && this.Scale.Equals(otherSpec.Scale);
        }

        public override int GetHashCode() {
            unchecked {
                var hashcode = 17;
                if (this.DbTypeName != null) {
                    hashcode += 34 * this.DbTypeName.GetHashCode();
                }

                hashcode += this.Length.GetHashCode() * 34;
                hashcode += this.Precision.GetHashCode() * 34;
                hashcode += this.Scale.GetHashCode() * 34;
                return hashcode;
            }
        }
    }
}