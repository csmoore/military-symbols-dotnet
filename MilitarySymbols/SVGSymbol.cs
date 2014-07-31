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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

namespace MilitarySymbols
{
    public class SvgSymbol
    {
        public static Size ImageSize 
        { 
            get { return defaultImageSize; }
            set { defaultImageSize = value; }
        }
        private static Size defaultImageSize = new Size(256, 256);

        public static Bitmap GetBitmap(List<string> graphicLayers)
        {
            if (graphicLayers.Count == 0)
                return null;

            if (ImageSize == null)
                ImageSize = new Size(256, 256);

            Bitmap bitmap = new Bitmap(ImageSize.Width, ImageSize.Height);

            foreach (string graphicLayer in graphicLayers)
            {
                if (!System.IO.File.Exists(graphicLayer))
                {
                    System.Diagnostics.Trace.WriteLine("Could not find SVG layer: "
                        + graphicLayer);
                    continue;
                }

                // Uncomment if you want to see the layers drawn:
                // System.Diagnostics.Debug.WriteLine("Drawing SVG layer: " + graphicLayer);

                try
                {
                    SvgDocument document = GetSvgDoc(graphicLayer);

                    document.Draw(bitmap);
                }
                catch (Exception ex)
                {
                    // TODO: figure out why SVG is now crashing on some (ex. 25321100)
                    System.Diagnostics.Trace.WriteLine("Exception drawing SVG layer: " + graphicLayer);
                    System.Diagnostics.Trace.WriteLine("Message: " + ex.Message);
                    bitmap = null;
                    break;
                }
            }

            return bitmap;
        }

        public static SvgDocument GetSvgDoc(string svgFile)
        {
            SvgDocument document = SvgDocument.Open(svgFile);

            return Resize(document);
        }

        private static SvgDocument Resize(SvgDocument document)
        {
            // Make document have same dimensions as image
            if (document.Height != ImageSize.Height)
            {
                document.Width = ImageSize.Width; 
                document.Height = ImageSize.Height;
            }
            return document;
        }

    }

}
