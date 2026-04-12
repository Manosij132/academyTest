export interface ChatMessage {
  text: string;
  sender: 'user' | 'bot';
  data?: any[];
  type: string;
  suggestedPromtMessage?:any[];
}