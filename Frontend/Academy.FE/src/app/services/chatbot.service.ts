import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from "../../environments/environment";
import { ApiConstants } from "../shared/constants/api.constants";
import { query } from 'express';


@Injectable({
  providedIn: "root",
})      

export class ChatBotService{
    private apiUrl: string;
    private staffingApiUrl: string;
    
  constructor(private http: HttpClient) {
      this.apiUrl = environment.apiBaseUrl + "/" + environment.apiExtension;
      this.staffingApiUrl = environment.staffingApiBaseurl;
  }
  
  PostMessage(request: any) {
      const url = `${this.apiUrl}/${ApiConstants.CTRL_CHATBOT}/${ApiConstants.PATH_GET_REPLY}?query=${request.message}`;
      return this.http.get(url);
  } 

  StaffingGetMessage(request: any) {
      const url = `${this.apiUrl}/${ApiConstants.CTRL_CHATBOT}/${ApiConstants.PATH_GET_REPLY}?query=${request.message}`;
      const conversationID : string | null = sessionStorage.getItem("ConversationID") ?? "";
      let httpHeaders = new HttpHeaders().set("ConversationID", conversationID)
      return this.http.get(url, { headers: httpHeaders });
  }
  
  AcademyChatBotMessage(request:any){
      const url = `${this.apiUrl}/${ApiConstants.CTRL_CHATBOT}/${ApiConstants.PATH_ACADEMY_GETACADEMYDATA}?query=${request.message}`;
      const conversationID : string | null = sessionStorage.getItem("ConversationID") ?? "";
      let httpHeaders = new HttpHeaders().set("ConversationID", conversationID)
      return this.http.get(url, { headers: httpHeaders });
  }

  StaffingChatBotMessage(request:any){
      const url = `${this.staffingApiUrl}/${ApiConstants.PATH_STAFFING}?query=${request.message}`;
      const conversationID : string | null = sessionStorage.getItem("ConversationID") ?? "";
      let httpHeaders = new HttpHeaders().set("ConversationID", conversationID)
      return this.http.get(url, { headers: httpHeaders });
  }
  
  PostTraningAssign(request: any) {
    const url = `${this.apiUrl}/${ApiConstants.CTRL_CHATBOT}/${ApiConstants.PATH_ASSIGN_TRAINING}`;
    return this.http.post(url, request);
  } 
}