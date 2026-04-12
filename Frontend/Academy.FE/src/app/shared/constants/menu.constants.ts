import { IMenu } from "../Interface/menu";

export const MENU_LIST: Readonly<IMenu[]> = [
  {
    icon: "fas fa-desktop",
    text: "Tracker List",
    routerLink: "/list",
    children: [],
    activeRoutes: ["/list"],
  },
  {
    icon: "fas fa-briefcase",
    text: "Training Impact",
    routerLink: "/training-impact",
    children: [],
    hasPermission: true,
    activeRoutes: ["/training-impact"],
  },
  {
    icon: "fas fa-layer-group",
    text: "Trainings",
    children: [
      {
        icon: "",
        text: "Manage",
        routerLink: "/trainings/manage",
        activeRoutes: ["/trainings/manage"],
      },
      {
        icon: "",
        text: "Spin",
        routerLink: "/trainings/spin",
        activeRoutes: ["/trainings/spin"],
      },
    ],
    hasPermission: true,
    isCollapsible: true,
    collapseId: "trainings",
    activeRoutes: ["/trainings/manage", "/trainings/spin"],
  },
  {
    icon: "fas fa-file",
    text: "Export Reports",
    children: [
      {
        icon: "",
        text: "Dojo Engagement Report",
        routerLink: "/reports/dojoactivity",
        activeRoutes: ["/reports/dojoactivity"],
      },
      {
        icon: "",
        text: "Full Report",
        routerLink: "/reports/fullreport",
        activeRoutes: ["/reports/fullreport"],
      }
    ],
    hasPermission: true,
    isCollapsible: true,
    collapseId: "reports",
    activeRoutes: ["/reports/dojoactivity","/reports/fullreport"],
  },
  {
    icon: "fas fa-file",
    text: "Training Reports",
    routerLink: "/trainingreportbookmarks",
    children: [],
    activeRoutes: ["/trainingreportbookmarks"],
  },
  {
    icon: "fas fa-layer-group",
    text: "Admin",
    children: [
      {
        icon: "",
        text: "Manage Role",
        routerLink: "/adm/manage/role",
        activeRoutes: ["/adm/manage/role"],
      }
    ],
    hasPermission: true,
    isCollapsible: true,
    collapseId: "admin",
    activeRoutes: ["/adm/manage/role"],
  }
];
