export interface EmailAccount {
  id?: string;
  displayName: string;
  email: string;
  host: string;
  username: string;
  password: string;
  port: number;
  enableSsl: boolean;
  isDefaultEmailAccount: boolean;
}
