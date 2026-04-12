import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { PanelService } from '@services/panel.service';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { SendEmailModel } from '../../model/send-email.model';
import { AuthenticationService } from '@services/authentication.service';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'mf-app-panel-send-email-popup',
  templateUrl: './panel-send-email-popup.component.html',
  styleUrls: ['./panel-send-email-popup.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule
  ]
})
export class PanelSendEmailPopupComponent implements OnInit {

  
  fromEmail: string = '';
  sendEmailModel: SendEmailModel = {
    fromEmail: '',
    globerEmail: '',
    globerLeaderEmail: '',
    subject: '',
    body: '',
    communityGKFocalEmailId: ''
  };

  constructor(private panelService:PanelService, private dialogRef: MatDialogRef<PanelSendEmailPopupComponent>, 
    @Inject(MAT_DIALOG_DATA)  public data: any, private authenticationService: AuthenticationService) { }

  ngOnInit(): void {
    var user = this.authenticationService.getUserSessionDetails();
    this.fromEmail = user !=null && user != undefined ? user.email : "";
    this.sendEmailModel.fromEmail = this.fromEmail;
    this.sendEmailModel.globerEmail = this.data.panel.globerEmail;
    this.sendEmailModel.globerLeaderEmail = this.data.panel.globerLeaderEmail +  (this.data.panel.communityGKFocalEmailId ?   ", " + this.data.panel.communityGKFocalEmailId : "");
    this.sendEmailModel.subject = '';
    this.sendEmailModel.body = '';
    this.sendEmailModel.communityGKFocalEmailId = this.data.panel.communityGKFocalEmailId
  }

  sendEmail(panelSendEmail: any){
    this.panelService.panelSendEmail(panelSendEmail).subscribe((res) => {
      if(res)
        {
          alert('Email sent Successfully');
        }
        else
        {
          alert("Something went wrong, please connect with administrator");
        }
    });
  }
  
}
