export interface SubMenuItem {
    name: string;
    path: string
    icon: string;
}

export const subMenuItems: SubMenuItem[] = [
    { name: 'Tickets Tracker', path: 'list-of-tickets', icon: 'fas fa-list-alt' },
    { name: 'Tickets Summary', path: 'summary', icon: 'fas fa-clipboard' },
];

