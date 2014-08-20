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
    /// <summary>
    /// Container/aggregator for all of the attributes of a 2525D Military Symbol 
    /// Ex: SymbolIdCode, GraphicLayer, Tags, ShapeType, etc.
    /// </summary>
    public class MilitarySymbol
    {
        public SymbolIdCode Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        protected SymbolIdCode id = SymbolIdCode.DefaultSymbolIdCode;

        public string Legacy2525Code
        {
            get
            {
                string code2525Charlie;
                bool convertSuccess = Utilities.ConvertCodeDeltaToCharlie(Id, out code2525Charlie);
                if (convertSuccess)
                    return code2525Charlie;
                else
                    return string.Empty;
            }
            set
            {
                // WARNING: this will change the SymbolId/State of the Symbol
                string legacyCode = value;

                System.Diagnostics.Trace.WriteLine("Setting MilitarySymbol.SymbolId from legacy code: " + legacyCode);

                SymbolIdCode newCode;
                bool success = Utilities.ConvertCodeCharlieToDelta(legacyCode, out newCode);

                if ((success) && (newCode.IsValid))
                    this.Id = newCode;
                else
                    System.Diagnostics.Trace.WriteLine("Could not create MilitarySymbol.SymbolId with legacy code: " + legacyCode);
            }
        }

        public ShapeType Shape
        {
            get 
            {
                shape = Utilities.GetShapeForId(this.Id);

                return shape;
            }
            set 
            {
                shape = value;
            }
        }
        protected ShapeType shape = ShapeType.Unknown;

        public CentralIconType CentralIconFormat
        {
            get 
            {
                centralIconFormat = Utilities.GetCentralIconForId(this.Id);

                return centralIconFormat;
            }
            set 
            {
                centralIconFormat = value;
            }
        }
        protected CentralIconType centralIconFormat = CentralIconType.NotSet;

        public List<string> GraphicLayers
        {
            get
            {
                return graphicLayers;
            }
        }
        protected List<string> graphicLayers = new List<string>();

        public List<string> Tags
        {
            get
            {
                tags.Clear();

                if (this.id != null)
                    tags.AddRange(this.id.Tags);

                if (this.Shape != ShapeType.Unknown)
                    tags.Add(this.Shape.ToString());

                string legacy2525Code = this.Legacy2525Code;
                if (!string.IsNullOrEmpty(legacy2525Code))
                    tags.Add(legacy2525Code);

                if (CentralIconFormat != CentralIconType.NotSet)
                    tags.Add(this.CentralIconFormat.ToString());

                // TODO: Add Other Desired Tags

                if (this.Id.IsValid)
                    tags.Add(this.Id.ToString());

                return tags;
            }
        }
        protected List<string> tags = new List<string>();

        public string TagsAsString 
        {
            get
            {
                StringBuilder tagBuilder = new StringBuilder();

                foreach (string tag in Tags)
                {
                    // Not sure if we need to do this:
                    // tagBuilder.Append(tag.Replace('_', ' '));
                    tagBuilder.Append(tag);
                    // TODO: ";" was the old tag delimitter (check that this is still true)
                    tagBuilder.Append(";"); 
                }

                return tagBuilder.ToString();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SIDC:");
            sb.Append(this.Id.ToString());
            sb.Append(":Geometry:");
            sb.Append(this.Shape);

            if (GraphicLayers.Count > 0)
            {
                sb.Append(":GraphicLayers:");
                foreach (string gl in GraphicLayers)
                {
                    sb.Append(gl);
                    sb.Append(":");
                }
            }

            if (!string.IsNullOrEmpty(Legacy2525Code))
            {
                sb.Append("2525C:");
                sb.Append(Legacy2525Code);
            }

            // Change if you want to include
            bool INCLUDE_TAGS = false;
            if (INCLUDE_TAGS)
            {
                List<string> writeTags = Tags;
                if (writeTags.Count > 0)
                {
                    sb.Append(":Tags:");
                    foreach (string s in writeTags)
                    {
                        sb.Append(s);
                        sb.Append(":");
                    }
                }
            }

            return sb.ToString();
        }

        public override bool Equals(System.Object obj)
        {
            if ((System.Object)obj == null)
                return false;

            MilitarySymbol ms = obj as MilitarySymbol;
            if ((System.Object)ms == null)
                return false;

            return Equals(ms);
        }

        public bool Equals(MilitarySymbol ms)
        {
            if ((System.Object)ms == null)
                return false;

            return ((Id == ms.Id) && (Shape == ms.Shape));
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Shape.GetHashCode();
        }

        public static bool operator ==(MilitarySymbol ms1, MilitarySymbol ms2)
        {
            return ms1.Equals(ms2);
        }

        public static bool operator !=(MilitarySymbol ms1, MilitarySymbol ms2)
        {
            return !ms1.Equals(ms2);
        }   

    }
}
