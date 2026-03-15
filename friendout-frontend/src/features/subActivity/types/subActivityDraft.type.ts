export interface SubActivityDraft {
  id: string;
  name: string;
  startTime: string;
  endTime: string | null;
  description: string | null;
  price: number | null;
}
