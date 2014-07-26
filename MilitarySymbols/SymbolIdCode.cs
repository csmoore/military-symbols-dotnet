/* 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilitarySymbols
{
    public class SymbolIdCode
    {
        public bool IsValid 
        { 
            get 
            { 
                // TODO: Use Regex or some other method to see if format good, 
                // or check properties are set, just check 1 property for now
                return (this.SymbolSet != SymbolSetType.NotSet); 
            }
        }

        public static SymbolIdCode DefaultSymbolIdCode
        {
            get
            {
                // TODO: detetmine if this is the behavior we want(just creates a new one every time)
                return new SymbolIdCode();
            }
        }

        /// <summary>
        /// The Full 20-digit code
        /// </summary>
        public string Code
        {
            get
            {
                populateCodeFromProperties();
                return code;
            }
            set
            {
                // if the value has changed
                if (code != value)
                {
                    // just a check to make sure this is only digits
                    // IMPORTANT/TODO: we aren't catching this exception if it fails
                    int convertCheck = Convert.ToInt32(code);

                    // TODO: add any other checks (e.g. Regex?) desired 

                    code = value;

                    // Populate the objects field from this code
                    populatePropertiesFromCode();
                }
            }
        }
        protected string code = null;

        /// <summary>
        /// The most significant portions of the Code: symbol set, entity & modifier (if set)
        /// </summary>
        public string ShortenedCode
        {
            get
            {
                StringBuilder sbCode = new StringBuilder();
                // SymbolSet (Digits 5 & 6)
                sbCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.SymbolSet, 2));
                // EntityCode (Digit 11-16)
                sbCode.Append(EntityCode);

                if (ModifierOne != "00")
                    sbCode.Append(ModifierOne);

                if (ModifierTwo != "00")
                    sbCode.Append(ModifierOne);

                return sbCode.ToString();
            }
        }

        // HACK: Having this "Name" Attribute here is somewhat of a hack
        // It just allows a readable form of this to be set like:
        // "SymbolSet : Entity : EntityType : EntitySubType : Modifier1 : Modifier2"
        // "Air : Military : Fixed Wing : Bomber : Light"
        // TODO: Find a better way to have this set automatically 
        // The probem is that only the SymbolLookup class knows how to lookup the Names from the 
        // codes (which it does in SymbolLookup.CreateSymbolFromStringProperties)
        // so we would need to add a (circular) dependency (& that probably isn't a good idea)
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;

                // HACK: TODO: fix - remove any empy " : "
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Replace(" :  :  : ", " : ");
                    name = name.Replace(" :  : ", " : ");
                }
            }
        }
        protected string name = null;

        ///////////////////////////////////////////////////////////
        // 2525D: A.5.2.1  Set A - First ten digits 
        // Version (Digits 1 and 2) 
        // Standard identity 1, Standard identity 2(AffiliationType) (Digits 3 and 4)
        // Symbol set (Digits 5 and 6)
        // Status (Digit 7)
        // HQ/Task Force/Dummy (Digit 8)
        // Amplifier/Descriptor (Digits 9 and 10)

        public StandardVersionType StandardVersion // Digits (1 & 2)
        {
            get { return standardVersion; }
            // TODO: allow these to be set later, only allow the default for now
        }
        protected StandardVersionType standardVersion = StandardVersionType.Current2525D;

        public StandardIdentityRealExerciseSimType StandardIdentity // StandardIdentity 1 (Digit 3)
        {
            get { return standardIdentity; }
            set { standardIdentity = value; }
        }
        protected StandardIdentityRealExerciseSimType standardIdentity = 
            StandardIdentityRealExerciseSimType.Reality;

        public StandardIdentityAffiliationType Affiliation  // StandardIdentity 2 (Digit 4)
        {
            get
            {
                return affiliation;
            }
            set
            {
                affiliation = value;
            }
        }
        protected StandardIdentityAffiliationType affiliation = StandardIdentityAffiliationType.Unknown;

        public SymbolSetType SymbolSet // (Digits 5 & 6)
        {
            get
            {
                return symbolSet;
            }
            set
            {
                symbolSet = value;
            }
        }
        protected SymbolSetType symbolSet = SymbolSetType.NotSet;

        public string SymbolSetAsString // (Digits 5 & 6)
        {
            get
            {
                return TypeUtilities.EnumHelper.getEnumValAsString(symbolSet);
            }
            set
            {
                string symbolSetAsString = value;
                symbolSet = (SymbolSetType)TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetAsString); 
            }
        }

        public StatusType Status // (Digit 7)
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
        protected StatusType status = StatusType.Present;

        public HeadquartersTaskForceDummyType HeadquartersTaskForceDummy // (Digit 8)
        {
            get
            {
                return headquartersTaskForceDummy;
            }
            set
            {
                headquartersTaskForceDummy = value;
            }
        }
        protected HeadquartersTaskForceDummyType headquartersTaskForceDummy 
            = HeadquartersTaskForceDummyType.NoHQTFDummyModifier;

        public EchelonMobilityType EchelonMobility    // Amplifier 1, 2 (Digit 9 & 10)
        {
            get
            {
                return echelonMobility;
            }
            set
            {
                echelonMobility = value;
            }
        }
        protected EchelonMobilityType echelonMobility = EchelonMobilityType.NoEchelonMobility;

        //
        ///////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////
        // 2525D: A.5.2.2  Set B - Second ten digits
        // Entity (Digits 11 and 12)
        // Entity type (Digits 13 and 14)
        // Entity subtype (Digits 15 and 16)
        // Sector 1 modifier (Digits 17 and 18)
        // Sector 2 modifier (Digits 19 and 20)
        //

        public string EntityField //  (Digit 11-12)
        {
            get
            {
                return entity;
            }
            set
            {
                string checkString = value;
                // An unfortunate side-effect of having 2 similarly-named "Entity"
                if (checkString.Length == 6)
                    System.Diagnostics.Trace.WriteLine("Appear to be setting wrong field: Use `EntityCode`");

                entity = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string entity = "00";

        public string EntityTypeField //  (Digit 13-14)
        {
            get
            {
                return entityType;
            }
            set
            {
                string checkString = value;
                entityType = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string entityType = "00";

        public string EntitySubTypeField //  (Digit 13-14)
        {
            get
            {
                return entitySubType;
            }
            set
            {
                string checkString = value;
                entitySubType = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string entitySubType = "00";

        /// <summary>
        /// The FULL/Complete 6-character Entity Code (Digit 11-16)  
        /// </summary>
        public string EntityCode 
        {
            get
            {
                return EntityField + EntityTypeField + EntitySubTypeField;
            }
            set
            {
                string checkString = value;
                string fullCode = TypeUtilities.ValidateAndZeroPad(checkString, 6);
                EntityField        = fullCode.Substring(0, 2);
                EntityTypeField    = fullCode.Substring(2, 2);
                EntitySubTypeField = fullCode.Substring(4, 2);
            }
        }

        public string ModifierOne //  (Digit 17-18)
        {
            get
            {
                return modifierOne;
            }
            set
            {
                string checkString = value;
                modifierOne = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string modifierOne = "00";

        public string ModifierTwo //  (Digit 19-20)
        {
            get
            {
                return modifierTwo;
            }
            set
            {
                string checkString = value;
                modifierTwo = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string modifierTwo = "00";
        ///////////////////////////////////////////////////////////

        public override string ToString()
        {
            return convertToString(true);
        }

        public List<string> Tags
        {
            get
            {
                tags.Clear();

                if (this.Affiliation != StandardIdentityAffiliationType.NotSet)
                    tags.Add(this.Affiliation.ToString());

                if (this.SymbolSet != SymbolSetType.NotSet)
                    tags.Add(TypeUtilities.EnumHelper.getStringFromEnum(this.SymbolSet));

                if (this.Status != StatusType.NotSet)
                    tags.Add(this.Status.ToString());

                if (this.HeadquartersTaskForceDummy != HeadquartersTaskForceDummyType.NoHQTFDummyModifier)
                    tags.Add(this.HeadquartersTaskForceDummy.ToString());

                if (this.EchelonMobility != EchelonMobilityType.NoEchelonMobility)
                    tags.Add(this.EchelonMobility.ToString());

                if (!string.IsNullOrEmpty(this.Name) && this.Name.Length > 0)
                    tags.Add(this.Name);

                return tags;
            }
        }
        List<string> tags = new List<string>();

        public string CodeFirstTen
        {
            get
            {
                return convertToStringFirst10(false); ;
            }
        }

        public string CodeSecondTen
        {
            get
            {
                return convertToStringSecond10(false);
            }
        }

        public string CodeFirstTenFormatted
        {
            get
            {
                return convertToStringFirst10(true); ;
            }
        }

        public string CodeSecondTenFormatted
        {
            get
            {
                return convertToStringSecond10(true); 
            }
        }

        private string convertToStringFirst10(bool formatted = false)
        {
            StringBuilder sbSymbolIdCode = new StringBuilder();

            // Digits 1-10

            if (formatted)
                sbSymbolIdCode.Append("(");

            // StandardVersion - Digits (1 & 2)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.StandardVersion));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // StandardIdentity 1 - Real/Exercise/Sim (Digit 3)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.StandardIdentity));

            // StandardIdentity 2 - Affiliation (Digit 4)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.Affiliation));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // SymbolSet (Digits 5 & 6)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.SymbolSet, 2));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // Status (Digit 7) 
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.Status));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // HeadquartersTaskForceDummy (Digit 8)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.HeadquartersTaskForceDummy));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // EchelonMobility    // Amplifier 1, 2 (Digit 9 & 10)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.EchelonMobility, 2));

            if (formatted)
                sbSymbolIdCode.Append(")");

            return sbSymbolIdCode.ToString();
        }

        private string convertToStringSecond10(bool formatted = false)
        {
            StringBuilder sbSymbolIdCode = new StringBuilder();

            // Digits 11-20

            if (formatted)
                sbSymbolIdCode.Append("(");

            // EntityCode (Digit 11-16)
            sbSymbolIdCode.Append(EntityCode);

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // ModifierOne (Digit 17-18)
            sbSymbolIdCode.Append(ModifierOne);

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // ModifierTwo (Digit 19-20)
            sbSymbolIdCode.Append(ModifierTwo);

            if (formatted)
                sbSymbolIdCode.Append(")");

            return sbSymbolIdCode.ToString();
        }

        private string convertToString(bool formatted = false)
        {
            StringBuilder sbSymbolIdCode = new StringBuilder();

            // Digits 1-10
            if (formatted)
                sbSymbolIdCode.Append("( ");

            sbSymbolIdCode.Append(convertToStringFirst10(formatted));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // Digits 11-20
            sbSymbolIdCode.Append(convertToStringSecond10(formatted));

            if (formatted)
                sbSymbolIdCode.Append(" )");

            return sbSymbolIdCode.ToString();
        }

        private void populateCodeFromProperties()
        {
            // TODO: we may not want to do this every time, but only
            // when a property has changed
            code = convertToString(false);
        }

        private void populatePropertiesFromCode()
        {
            if (code.Length < 20)
                return;

            string standardIdentityString = code.Substring(2, 1);
            this.StandardIdentity = (StandardIdentityRealExerciseSimType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StandardIdentityRealExerciseSimType), standardIdentityString);

            string affiliationString = code.Substring(3, 1);
            this.Affiliation = (StandardIdentityAffiliationType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StandardIdentityAffiliationType), affiliationString);

            string symbolSetString = code.Substring(4, 2);
            this.SymbolSet = (SymbolSetType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetString);

            string statusString = code.Substring(6, 1);
            this.Status = (StatusType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StatusType), statusString);

            string hqTFFDString = code.Substring(7, 1);
            this.HeadquartersTaskForceDummy = (HeadquartersTaskForceDummyType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(HeadquartersTaskForceDummyType), hqTFFDString);

            string echelonMobilityString = code.Substring(8, 2);
            this.EchelonMobility = (EchelonMobilityType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(EchelonMobilityType), echelonMobilityString);

            this.EntityCode = code.Substring(10, 6);
            this.ModifierOne  = code.Substring(16, 2);
            this.ModifierTwo = code.Substring(18, 2);
        }

        public override bool Equals(System.Object obj)
        {
            if ((System.Object)obj == null)
                return false;

            SymbolIdCode sidc = obj as SymbolIdCode;
            if ((System.Object)sidc == null)
                return false;

            return Equals(sidc);
        }

        public bool Equals(SymbolIdCode sidc)
        {
            if ((System.Object)sidc == null)
                return false;

            return (Code == sidc.Code);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public static bool operator ==(SymbolIdCode sidc1, SymbolIdCode sidc2)
        {
            return sidc1.Equals(sidc2);
        }

        public static bool operator !=(SymbolIdCode sidc1, SymbolIdCode sidc2)
        {
            return !sidc1.Equals(sidc2);
        }   

    }

}
