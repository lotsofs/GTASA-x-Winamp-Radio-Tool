﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTASARadioExternal {
    public partial class FormRE : Form {

        Radio radio;
        
        public FormRE() {
            InitializeComponent();

            radio = new Radio();
        }
    }
}