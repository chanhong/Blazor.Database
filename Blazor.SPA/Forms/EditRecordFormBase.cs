﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazor.SPA.Components;
using Blazor.SPA.Data;
using Blazor.SPA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blazor.SPA.Forms
{
    /// <summary>
    /// Abstract Class defining all the boilerplate code used in an edit form
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    public abstract class EditRecordFormBase<TRecord, TEditRecord> :
        RecordFormBase<TRecord>,
        IDisposable
        where TRecord : class, IDbRecord<TRecord>, new()
        where TEditRecord : class, IEditRecord<TRecord>, new()
    {
        [Inject] protected RouteViewService RouteViewService { get; set; }

        public virtual Guid FormId { get; } = Guid.Empty;

        protected EditContext EditContext { get; set; }

        protected bool IsDirty
        {
            get => this._isDirty;
            set
            {
                if (value != this.IsDirty)
                {
                    this._isDirty = value;
                    if (this._isModal) this.Modal.Lock(value);
                }
            }
        }

        protected TEditRecord Model { get; set; }

        protected EditFormState EditFormState { get; set; }

        // Set of boolean properties/fields used in the razor code and methods to track 
        // state in the form or disable/show/hide buttons.
        protected bool _isNew => this.Service?.IsNewRecord ?? true;
        protected bool _isDirty { get; set; } = false;
        protected bool _isValid { get; set; } = true;
        protected bool _saveDisabled => !this.IsDirty || !this._isValid;
        protected bool _deleteDisabled => this._isNew || this._confirmDelete || this.IsDirty;
        protected bool _isLoaded { get; set; } = false;
        protected bool _dirtyExit => this._isDirty;
        protected bool _confirmDelete { get; set; } = false;
        protected bool _isInlineDirty => (!this._isModal) && this._isDirty;
        protected string _saveButtonText => this._isNew ? "Save" : "Update";

        protected override ComponentState LoadState => _isLoaded ? ComponentState.Loaded : ComponentState.Loading;

        protected override async Task LoadRecordAsync()
        {
            _isLoaded = false;
            this.RouteViewService.EditFormId = this.FormId;
            this.RouteViewService.EditForm = this.GetType();
            this.TryGetModalID();
            var id = this._Id;
            // check if we have a saved EditeState for this form and if so set the id
            if (this.GetEditStateId(out Guid editstateid))
                id = editstateid;

            await this.Service.GetRecordAsync(id);
                this.Model = new TEditRecord();
                this.Model.Populate(this.Service.Record);
            // assign the Model to the EditContext
            this.EditContext = new EditContext(this.Model);
            _isLoaded = true;
            this.EditContext.OnFieldChanged += FieldChanged;
            if (!this._isNew)
                this.EditContext.Validate();
        }

        protected void FieldChanged(object sender, FieldChangedEventArgs e)
        {
            this._confirmDelete = false;
        }

        protected void EditStateChanged(bool dirty)
            => this.IsDirty = dirty;

        protected void ValidStateChanged(bool valid)
            => this._isValid = valid;

        protected async Task HandleValidSubmit()
        {
            var rec = this.Model.GetRecord();
            await this.Service.SaveRecordAsync(rec);
            this.EditFormState.UpdateState();
            this.EditFormState.ClearEditState();
            await this.InvokeAsync(this.StateHasChanged);
        }

        protected void ResetToRecord()
        {
            this.Model.Populate(this.Service.Record);
            this.IsDirty = false;
        }

        protected void Delete()
        {
            if (!this._isNew)
                this._confirmDelete = true;
        }

        protected async Task ConfirmDelete()
        {
            if (this._confirmDelete)
            {
                await this.Service.DeleteRecordAsync();
                this.IsDirty = false;
                await this.DoExit();
            }
        }

        protected async Task ConfirmExit()
        {
            this.IsDirty = false;
            this.EditFormState.ClearEditState();
            await this.DoExit();
        }

        protected async Task DoExit(ModalResult result = null)
        {
            result ??= ModalResult.OK();
            if (this._isModal)
                this.Modal.Close(result);
            if (ExitAction.HasDelegate)
                await ExitAction.InvokeAsync();
            else
                this.NavManager.NavigateTo("/");
        }

        protected bool GetEditStateId(out Guid id)
        {
            var rec = RouteViewService.GetEditState(this.FormId);
            var hasRecord = rec != null;
            if (hasRecord)
            {
                var data = JsonSerializer.Deserialize<TEditRecord>(rec.Data);
                var model = (TEditRecord)data;
                id = model.ID;
                return true;
            }
            else
            {
                id =  Guid.Empty;
                return false;
            }
        }

        public void Dispose()
        {
            if (this.EditContext != null)
                this.EditContext.OnFieldChanged -= FieldChanged;
        }
    }
}