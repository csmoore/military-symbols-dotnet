﻿/* 
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MilSymbolPicker
{
    public partial class FormExtras : Form
    {
        public FormExtras()
        {
            InitializeComponent();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK; // have to set this in case call from code, not button
            this.Close();
        }

        private void cbManuallyEnterCode_CheckedChanged(object sender, EventArgs e)
        {
            this.tbManuallyEnterCode.Visible = cbManuallyEnterCode.Checked;

            if (cbManuallyEnterCode.Checked)
                tbManuallyEnterCode.Focus();
        }

        private void tbManuallyEnterCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                butOK_Click(this, null);
        }
    }
}
