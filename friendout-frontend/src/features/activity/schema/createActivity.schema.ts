import { z } from "zod";
import { LocalisationType } from "@/features/localisation/types/localisation.type";

// ─── Localisation ──────────────────────────────────────────────────────────
const localisationSchema = z.object({
  type: z.nativeEnum(LocalisationType),
  address: z.string().trim().optional(),
  mapLink: z.string().url().trim().optional(),
  virtualUrl: z.string().trim().optional(),
  serverInfo: z.string().trim().optional(),
}).refine((data) => {
  if (data.type === LocalisationType.Address) return !!data.address?.trim();
  if (data.type === LocalisationType.MapLink) return !!data.mapLink?.trim();
  if (data.type === LocalisationType.Virtual) return !!(data.virtualUrl?.trim() || data.serverInfo?.trim());
  return true;
}, { message: "location_incomplete" });

// ─── Sub-activité ──────────────────────────────────────────────────────────
const subActivitySchema = z.object({
  id: z.string().optional().nullable(),
  name: z.string().trim().min(1, "name_required").min(2, "name_too_short"),
  startTime: z.string().min(1, "time_required").regex(/^\d{2}:\d{2}$/, "time_invalid_format"),
  endTime:   z.string().min(1, "time_required").regex(/^\d{2}:\d{2}$/, "time_invalid_format"),
  description: z.string().trim().optional().nullable(),
  price: z.number().min(0).optional().default(0),
  localisation: localisationSchema.optional().nullable(),
}).refine(
  (data) => {
    const toMinutes = (t: string) => {
      const [h, m] = t.split(":").map(Number);
      return Number.isFinite(h) && Number.isFinite(m) ? h * 60 + m : -1;
    };
    const start = toMinutes(data.startTime);
    const end   = toMinutes(data.endTime);
    return start === -1 || end === -1 || end > start;
  },
  { message: "end_before_start", path: ["endTime"] }
);

// ─── Schéma de base ────────────────────────────────────────────────────────
const baseSchema = z.object({
  title:       z.string().trim().min(1, "title_required").min(3, "title_too_short"),
  description: z.string().trim().min(1, "description_required").min(10, "description_too_short"),
  startAt:     z.date({ message: "date_required" }),
  time:        z.string().min(1, "time_required").regex(/^\d{2}:\d{2}$/, "time_invalid_format"),
  endAt:       z.date().optional(),
  estimatedPrice: z.number().min(0).optional(),
  localisation: localisationSchema.nullable(),
  activityImage: z.instanceof(File).optional(),
  requiredEquipmentNames: z.array(z.string().trim().min(1)).optional().default([]),
  subActivities: z.array(subActivitySchema).optional().default([]),
}).refine(
  (data) => data.localisation !== null,
  { message: "location_required", path: ["localisation"] }
);

// ─── Schéma contextuel selon le mode ──────────────────────────────────────
// En mode "create", on vérifie que la date est dans le futur.
// En mode "edit", cette contrainte n'existe pas (l'activité peut déjà être passée).
export const buildActivitySchema = (mode: "create" | "edit") => {
  if (mode === "edit") return baseSchema;

  return baseSchema.superRefine((data, ctx) => {
    if (data.startAt <= new Date()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "date_must_be_future",
        path: ["startAt"],
      });
    }
  });
};

export type CreateActivityFormData = z.infer<typeof baseSchema>;
