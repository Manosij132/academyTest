import { Component } from '@angular/core';
import { ChatMessage } from '@shared/dto/chat-message';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ChatBotService } from "@services/chatbot.service";
import { DialogService } from '@services/dialog.service';
import { TableViewService } from '@services/table-view.service';
import { FullTableViewComponent } from '@shared/component/full-table-view/full-table-view.component';
import { MatDialog } from '@angular/material/dialog';
import { v4 as uuidv4 } from 'uuid';
import { Agents } from '@shared/constants/app.constants';

@Component({
  selector: 'app-chat-ui',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './chat-ui.component.html',
  styleUrl: './chat-ui.component.css'
})

export class ChatUIComponent {
  fullTableData: any[] = [];
  showFullTable: boolean = false;
  currentPage = 1;
  page = 1;
  pageSize = 5;
  totalPages = 0;
  pages: number[] = [];
  data: any[] = [];
  searchText: string = '';
  filteredList: any[] = [];
  originalList: any[] = [];
  rowsCount = 6;
  ColumnsCount = 7;

  constructor(private dialogService: DialogService,
    private readonly chatBotService: ChatBotService,
    private tableViewService: TableViewService,
    private dialog: MatDialog
  ) { }

  activeMode: 'staffing' | 'academy' = 'staffing';
  userInput: string = '';  // This will hold the email entered by the user
  messages: any[] = [];    // This will store the chat messages
  trainingResponse: any[] = [];  // This will store the training data to be displayed in a table
  employees = [];
  isOpen = false; // by default open
  isFullScreen = false;
  isMinimized = false;
  isBotTyping = false;
  dragging = false;
  offsetX = 0;
  offsetY = 0;

  setActiveMode(mode: 'staffing' | 'academy') {
    this.activeMode = mode;
  }

  //   getColumns(data: any[]) {
  //    return Object.keys(data[0]);
  // }
  getColumns(data: any): string[] {
    try {
      const parsed = typeof data === 'string' ? JSON.parse(data) : data;
      return parsed?.length > 0 ? Object.keys(parsed[0]) : [];
    } catch (e) {
      console.error('getColumns error:', e);
      return [];
    }
  }
  tableSearch: string = '';  // bind to input

  filteredData(data: any): any[] {
    try {
      const parsed = typeof data === 'string' ? JSON.parse(data) : data;
      if (!parsed || parsed.length === 0 || !this.tableSearch?.trim()) {
        return parsed || [];
      }
      const term = this.tableSearch.toLowerCase();
      return parsed.filter((row: any) =>
        Object.values(row).some(v =>
          String(v).toLowerCase().includes(term)
        )
      );
    } catch {
      return [];
    }
  }

  filteredRows(data: any): number {
    return this.filteredData(data).length;
  }


  formatColumnName(col: string): string {
    return col.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  close() {
    this.isOpen = false;
    this.isFullScreen = false;
    this.messages = []; // Clear chat history on close
    this.userInput = '';
    if (sessionStorage.getItem("ConversationID") != null) sessionStorage.removeItem("ConversationID");

  }

  toggleFullScreen() {
    this.isFullScreen = !this.isFullScreen;

    if (this.isFullScreen) {
      this.isOpen = true;
      this.isMinimized = false;

      // Reset position styles when entering fullscreen
      const popup = document.querySelector('.chat-popup') as HTMLElement;
      if (popup) {
        popup.style.left = '';
        popup.style.top = '';
        popup.style.right = '';
        popup.style.bottom = '';
      }
    }
  }

  toggleMinimize() {
    this.isMinimized = !this.isMinimized;
    if (this.isMinimized && this.isFullScreen) {
      this.isFullScreen = false;
    }

    // If restoring from minimized, reset position
    if (!this.isMinimized) {
      const popup = document.querySelector('.chat-popup') as HTMLElement;
      if (popup) {
        popup.style.left = '100px';
        popup.style.top = '100px';
      }
    }
  }

  openChat() {
    // If chat is closed, open it
    if (!this.isOpen) {
      this.isOpen = true;
    }

    // Show welcome message only when chat is opened freshly
    if (this.messages.length === 0) {
      const welcomeText = `👋 Welcome to the Academy AI.
      
  You can:
  📌 Create trainings from ChatBot
  📊 Get training status completion
  ✅ Update Training Status
  👥 Assign new trainings to employees
  ⏳ Get the training status of an employee
  🧑‍💼 Enroll employees in trainings
  🗂️ Get all trainings Lists for EcoSystem
  🤖 Get all ai trainings
  🆔 Get all trainingIds Lists for EcoSystem
  🎰 Spin the traning`;

      this.messages.push({
        sender: 'bot',
        text: welcomeText,
        type: 'text'
      });

      if (sessionStorage.getItem("ConversationID") != null) sessionStorage.removeItem("ConversationID");
      sessionStorage.setItem("ConversationID", uuidv4());
    }

    // If it's minimized, restore it
    if (this.isMinimized) {
      this.isMinimized = false;
    }
  }

  startDrag(event: MouseEvent) {
    if (this.isFullScreen) return;

    this.dragging = true;
    const popup = document.querySelector('.chat-popup') as HTMLElement;
    const rect = popup.getBoundingClientRect();

    this.offsetX = event.clientX - rect.left;
    this.offsetY = event.clientY - rect.top;

    document.addEventListener('mousemove', this.onDrag);
    document.addEventListener('mouseup', this.stopDrag);
  }

  onDrag = (event: MouseEvent) => {
    if (!this.dragging || this.isFullScreen) return;

    const popup = document.querySelector('.chat-popup') as HTMLElement;
    const popupWidth = popup.offsetWidth;
    const popupHeight = popup.offsetHeight;

    // Calculate new positions
    let newLeft = event.clientX - this.offsetX;
    let newTop = event.clientY - this.offsetY;

    // Enforce window boundaries
    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;

    // Don't let popup go beyond screen edges
    newLeft = Math.max(0, Math.min(newLeft, windowWidth - popupWidth));
    newTop = Math.max(0, Math.min(newTop, windowHeight - popupHeight));

    popup.style.left = `${newLeft}px`;
    popup.style.top = `${newTop}px`;
    popup.style.right = 'auto';
    popup.style.bottom = 'auto';
  }

  stopDrag = () => {
    this.dragging = false;
    document.removeEventListener('mousemove', this.onDrag);
    document.removeEventListener('mouseup', this.stopDrag);
  }


  isLargeTable(data: any[]): boolean {
    if (!data || data.length === 0) return false;
    const columns = this.getColumns(data);
    return data.length > 5 || columns.length > 5;
  }
  getTableLink(msg: any): string {
    // You can dynamically create a route or link to a detailed view page
    // Example: /full-table/:id or use a blob link for downloaded content
    return `/full-table-view?id=${msg.id || ''}`;
  }
  openFullTable(msg: any) {
    this.tableViewService.setTableData(msg.data);
    sessionStorage.setItem('fullTableData', JSON.stringify(msg.data));
    //window.open('/full-table-view', '_blank');
    //this.fullTableData=msg.data;
    //this.showFullTable=true;
    this.dialog.open(FullTableViewComponent, {
      width: '1200px',  // modal width
      maxWidth: '100vw',
      height: '80%', // modal height
      data: {
        tableData: msg.data,        // pass your table data
        columns: Object.keys(msg.data[0] || {})  // derive columns from keys
      },

      panelClass: 'full-table-modal-panel', // ensures modal is above chat popup
      disableClose: false
    });
  }
  closeFullTable() {
    this.showFullTable = false;
    this.fullTableData = [];
  }
  toggleTable(msg: any) {
    msg.showTable = !msg.showTable;
  }
  //getPagedData(data:any[]){
  //const startIndex=(this.page-1)*this.pageSize;
  //return data.slice(startIndex,startIndex+this.pageSize);
  //}
  getPagedData(data: any): any[] {
    try {
      const parsed = typeof data === 'string' ? JSON.parse(data) : data;
      if (!parsed || parsed.length === 0) return [];
      const start = (this.page - 1) * this.pageSize;
      return parsed.slice(start, start + this.pageSize);
    } catch (e) {
      console.error('getPagedData error:', e);
      return [];
    }
  }

  hasTableData(data: any): boolean {
    try {
      const parsed = typeof data === 'string' ? JSON.parse(data) : data;
      return parsed?.length > 0 && Object.keys(parsed[0] || {}).length > 0;
    } catch {
      return false;
    }
  }

  trackByFn(index: number, item: any): any {
    return index;
  }


  nextPage(totalRows: number) {
    if (this.page * this.pageSize < totalRows) {
      this.page++;
    }
  }
  prevPage() {
    if (this.page > 1) {
      this.page--;
    }
  }
  trackByIndex(index: number): number {
    return index;
  }

  trackByCol(index: number, col: string): string {
    return col;
  }
  ngOnInit() {
    this.calculatePages;
  }
  calculatePages() {
    this.totalPages = Math.ceil(this.data.length / this.pageSize);
    this.pages = Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get paginatedData() {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.data.slice(start, start + this.pageSize);
  }

  goToPage(page: number) {
    this.currentPage = page;
  }

  previous() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  next() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }
  applyFilter() {
    const text = this.searchText.toLowerCase();
    this.filteredList = this.originalList.filter(item =>
      item.skillName.toLowercase().includes(text) ||
      item.trainingName.toLowerCase().includes(text) ||
      item.trainingIds.toString().includes(text)
    );
  }

  //send message function
  sendMessage(userInput: string) {
    if (!this.userInput.trim()) return;
    // Add user message

    this.messages.push({ text: `${this.userInput}`, sender: 'user' });
    const dom = {
      "userId": 1,
      "message": userInput
    }
    this.userInput = '';
    this.isBotTyping = true;


    this.chatBotService.StaffingGetMessage(dom).subscribe((result: any) => {

      var res;
      var responseData: any;
      var botMessage: ChatMessage = {
        text: '',
        sender: 'bot',
        type: 'Message'
      };
      this.isBotTyping = false;
      try {
        var agentName = result;
        if (agentName == Agents.Academy) {
          this.chatBotService.AcademyChatBotMessage(dom).subscribe((resultAcademy: any) => {
            responseData = resultAcademy;
            if (responseData.Value) responseData = responseData.Value;
            botMessage = {
              text: ('reply' in responseData) ? responseData.reply : responseData,
              sender: 'bot',
              data: ('data' in responseData) ? responseData.data : null,
              suggestedPromtMessage: ('suggestedPromt' in responseData) ? responseData.suggestedPromt : null,
              type: ('type' in responseData) ? responseData.type : 'Message'
            };
            this.messages.push(botMessage);
          });
        }
        else {
          this.chatBotService.StaffingChatBotMessage(dom).subscribe((resultAcademy: any) => {
            responseData = resultAcademy;
            if (responseData.Value) responseData = responseData.Value;
            botMessage = {
              text: ('Reply' in responseData) ? responseData.Reply : responseData,
              sender: 'bot',
              data: ('Data' in responseData) ? responseData.Data : null,
              suggestedPromtMessage: ('SuggestedPromt' in responseData) ? responseData.SuggestedPromt : null,
              type: ('Type' in responseData) ? responseData.Type : 'Message'
            };
            this.messages.push(botMessage);
          });
        }

      } catch (e) {
        res = result.response;
        botMessage = {
          text: res,
          sender: 'bot',
          type: 'Message'
        };
        this.messages.push(botMessage);
      }
    });


  }

  selectSuggestedPrompt(prompt: string) {
    this.userInput = prompt;
  }
}

function closeFullTable() {
  throw new Error('Function not implemented.');
}