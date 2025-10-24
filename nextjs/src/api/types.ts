export type User = {
  EmailAddress?: string;
  FirebaseId?: string;
  UserId: string;
  Name?: string;
  CreatedDate: string;
};

export type Role = {
  RoleId: string;
  Name?: string;
};

export type Permission = {
  PermissionId: string;
  Name?: string;
  Tag?: string;
};

export type Team = {
  TeamId: string;
  Name?: string;
};

export type TeamUser = {
  TeamUserId: string;
  TeamId: string;
  Team: string;
  UserId: string;
  User: string;
};

export type TeamRole = {
  TeamRoleId: string;
  TeamId: string;
  Team: string;
  RoleId: string;
  Role: string;
};

export type RolePermission = {
  RolePermissionId: string;
  RoleId: string;
  Role: string;
  PermissionId: string;
  Permission: string;
};

export type UserRole = {
  UserRoleId: string;
  UserId: string;
  User: string;
  RoleId: string;
  Role: string;
};

export type Option = {
  OptionId: string;
  Label?: string;
  Name?: string;
  Value?: string;
  Order?: number;
};

export type Setting = {
  SettingId: string;
  Name?: string;
  Value?: string;
};

export type System = {
  SystemId: string;
  Name?: string;
  Value?: string;
  DateTime?: string;
};

export type Server = {
  ServerId: string;
  Name: string;
  LastPing: string;
};

export type Job = {
  JobId: string;
  Name?: string;
  IsActive: boolean;
  Queue?: string;
  LastExecution: string;
  JobHistories: JobHistory[];
};

export type JobHistory = {
  JobHistoryId: string;
  Name: string;
  JobId: string;
  Job: string;
  Status: string;
  Message: string;
  CreatedDate: string;
  CompletedDate?: string;
  Ping: string;
  ServerName: string;
};

export type Audit = {
  AuditId: string;
  Name?: string;
  IsCreate: boolean;
  IsRead: boolean;
  IsUpdate: boolean;
  IsDelete: boolean;
  IsTable: boolean;
  AuditFields: AuditField[];
};

export type AuditField = {
  AuditFieldId: string;
  Name?: string;
  AuditId: string;
  Audit: string;
  IsCreate: boolean;
  IsUpdate: boolean;
  IsDelete: boolean;
};

export type AuditHistory = {
  AuditHistoryId: string;
  Name: string;
  TableName?: string;
  EntityId?: string;
  Operation: string;
  UserId?: string;
  User?: string;
  CreatedDate: string;
  Query?: string;
  Results?: string;
};

export type AuditHistoryDetail = {
  AuditHistoryDetailId: string;
  FieldName?: string;
  AuditHistoryId: string;
  AuditHistory: string;
  TableName?: string;
  FieldType?: string;
  OldValueId?: string;
  OldValue?: string;
  NewValueId?: string;
  NewValue?: string;
};

export type Log = {
  LogId: string;
  Timestamp: string;
  Level: string;
  Message: string;
  MessageTemplate?: string;
  SourceContext?: string;
  MachineName?: string;
  Environment?: string;
  ApplicationName?: string;
  Version?: string;
  ThreadId?: number;
  CorrelationId?: string;
  RequestId?: string;
  RequestPath?: string;
  RequestMethod?: string;
  StatusCode?: number;
  ElapsedMs?: number;
  UserId?: string;
  User?: string;
  UserName?: string;
  ClientIp?: string;
  UserAgent?: string;
  Exception?: string;
  ExceptionType?: string;
  ExceptionMessage?: string;
  Properties?: string;
  JobHistoryId?: string;
  JobHistory?: string;
};
