export interface MenuItem {
  name: string;
  path :string
  icon: string;
}

export const menuItems: MenuItem[] = [
//{ name: 'Candidates', path: 'candidates', icon: 'fas fa-user' },
  // { name: 'Seniorities', path: 'seniority', icon: 'fas fa-users' },
  { name: 'Skills', path: 'skill', icon: 'fas fa-lightbulb' },
  { name: 'Profiles', path: 'profile', icon: 'fas fa-user-circle' },
  { name: 'Questions', path: 'questions', icon: 'fas fa-hand-paper' },
  { name: 'Evaluations', path: 'evaluation', icon: 'fas fa-calendar-alt' },
  { name: 'AI Models', path: 'aiModel', icon: 'fas fa-microchip' },
  { name: 'Prompts', path: 'prompts', icon: 'fas fa-comment-alt' },
  // { name: 'Interview Analysis', path: 'interviewAnalysis', icon: 'fas fa-chart-line' },
  // { name: 'Interview Scoring', path: 'interviewScoring', icon: 'fas fa-star-half-alt' },
  { name: 'RabbitMQ' ,path:'rabbitMq',icon:'fas fa-star-half-alt'}
];

export const pauseInteviewConfirmationDialogue = {
  modalName: 'confirm_pause',
  title: 'Pause Evaluation?',
  message: 'You haven’t answered this question yet. If you continue, it will be skipped. Do you want to proceed?',
  confirmText: 'Yes, Pause',
  cancelText: 'Continue Answering'
}

export const nextQuestionConfirmationDialogue = {
  modalName: 'confirm_next',
  title: 'Skip Question?',
  message: 'You haven’t answered this question yet. If you continue, it will be skipped. Do you want to proceed?',
  confirmText: 'Yes, Skip',
  cancelText: 'Continue Answering'
}