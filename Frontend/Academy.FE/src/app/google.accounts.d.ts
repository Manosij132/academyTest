// google.accounts.d.ts
declare namespace google {
    namespace accounts {
      namespace id {
        interface CredentialResponse {
          credential?: string;
          clientId?: string;
          select_by?: string;
          client_id?: string;
        }
        interface PromptMomentNotification {
          getMomentType(): string;
        }
        interface IdConfiguration {
          client_id: string;
          auto_select?: boolean;
          login_uri?: string;
          callback?: (response: CredentialResponse) => void;
          nonce?: string;
          context?: string;
        }
        function initialize(config: IdConfiguration): void;
        function renderButton(
          element: HTMLElement | string,
          options: { theme?: string; size?: string; type?: string, text?: string, shape?: string, logo_alignment?: string}
        ): void;
        function prompt(callback?: (notification: PromptMomentNotification) => void): void;
      }
    }
  }