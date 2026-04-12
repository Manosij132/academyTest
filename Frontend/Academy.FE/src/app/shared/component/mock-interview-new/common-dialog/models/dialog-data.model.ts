import {TemplateRef} from "@angular/core";
import {FormGroup} from "@angular/forms";

export interface DialogData {
    title: string;
    message: string;
    showActions?: boolean;
    confirmText?: string;
    cancelText?: string;
    showCancel?: boolean;
    confirmButtonColor?: 'primary' | 'accent' | 'warn';
    form?: FormGroup;
    template?: TemplateRef<any>;
    isInvalidCandidateRef?: () => boolean;
}
