﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Microsoft.AspNetCore.Components;

namespace Blazor.UIComponents
{
    public class UILabelColumn : UIColumn
    {
        [Parameter] public override int Columns { get; set; } = 2;

        [Parameter] public string FormCss { get; set; } = "form-label";

        public UILabelColumn()
            => this.CssClasses.Add(this.FormCss);
    }
}