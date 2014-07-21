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
            get; set;
        }

        public ShapeType Shape
        {
            get
            {
                return shape;
            }
            set
            {
                shape = value;
            }
        }
        protected ShapeType shape = ShapeType.Unknown;

        public List<string> GraphicLayers
        {
            get
            {
                return graphicLayers;
            }
        }
        protected List<string> graphicLayers = new List<string>();

        public static bool FormatTagsForStyleFiles
        {
            get
            {
                return formatTagsForStyleFiles;
            }
            set
            {
                formatTagsForStyleFiles = value;
            }
        }
        private static bool formatTagsForStyleFiles = false;

        public List<string> Tags
        {
            get
            {
                tags.Clear();

                if (!formatTagsForStyleFiles)
                    if (this.id != null)
                        tags.AddRange(this.id.Tags);

                if (this.Shape != ShapeType.Unknown)
                    tags.Add(this.Shape.ToString());

                // TODO: Add Other Desired Tags


                // TRICKY: the old Tag format assumed that the SIDC was always last, 
                //         not sure if that is still the convention (but putting last just in case)
                if (!formatTagsForStyleFiles)
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

            if (Tags.Count > 0)
            {
                sb.Append(":Tags:");
                foreach (string s in Tags)
                {
                    sb.Append(s);
                    sb.Append(":");
                }
            }

            if (GraphicLayers.Count > 0)
            {
                sb.Append(":GraphicLayers:");
                foreach (string gl in GraphicLayers)
                {
                    sb.Append(gl);
                    sb.Append(":");
                }
            }

            return sb.ToString();
        }

    }
}
