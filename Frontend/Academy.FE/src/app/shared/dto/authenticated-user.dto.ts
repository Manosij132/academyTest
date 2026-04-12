export interface AuthenticatedUser {
  id?: number;
  globerEmail?: string;
  name?: string;
  roles?: Role[];
  community?: string;
  ecosystem?: string;
  careerMentorEmail?: string;
  userGexLeaderEmail?: string[];
  project?: string;
  client?: string;
  seniorityId?: number;
  seniority?: string;
  isAuthenticated?: boolean;
  gexLeaders?: string[];
  photoUrl?: string;
}

export interface Role {
  roleId?: number;
  roleName?: string;
  roleAssignment?: string;
  displayName?: string;
}
