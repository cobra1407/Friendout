import { z } from "zod";
import { LocalisationType } from "@/features/localisation/types/localisation.type";

// ─── Localisation ──────────────────────────────────────────────────────────
const localisationSchema = z.object({
  type: z.nativeEnum(LocalisationType),
  address: z.string().trim().nullish(),
  mapLink: z.string().url().trim().nullish(),
  virtualUrl: z.string().trim().nullish(),
  serverInfo: z.string().trim().nullish(),
}).refine((data) => {
  if (data.type === LocalisationType.Address) return !!data.address?.trim();
  if (data.type === LocalisationType.MapLink) return !!data.mapLink?.trim();
  // Virtual: virtualUrl and serverInfo are optional — address (server name) is sufficient.
  if (data.type === LocalisationType.Virtual) return !!(data.virtualUrl?.trim() || data.serverInfo?.trim() || data.address?.trim());
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

const baseSchema = z.object({
  title:       z.string().trim().min(1, "title_required").min(3, "title_too_short"),
  description: z.string().trim().min(1, "description_required").min(10, "description_too_short"),
  startAt:     z.date({ message: "date_required" }),
  time:        z.string().min(1, "time_required").regex(/^\d{2}:\d{2}$/, "time_invalid_format"),
  endAt:       z.date().optional(),
  estimatedPrice: z.number().min(0).optional(),
  localisation: localisationSchema.nullable(),
    removeImage: z.boolean().optional().default(false),
  activityImage: z.instanceof(File).optional(),
  requiredEquipmentNames: z.array(z.string().trim().min(1)).optional().default([]),
  subActivities: z.array(subActivitySchema).optional().default([]),
}).refine(
  (data) => data.localisation !== null,
  { message: "location_required", path: ["localisation"] }
);

export const buildActivitySchema = (mode: "create" | "edit", initialStartAt?: Date) => {
  if (mode === "create") {
    return baseSchema.superRefine((data, ctx) => {
      if (data.startAt <= new Date()) {
        ctx.addIssue({
          code: "custom",
          message: "date_must_be_future",
          path: ["startAt"],
        });
      }
    });
  }

  // Edit mode: only validate the date if the user actually changed it.
  // An existing past activity must remain editable (description, participants, etc.)
  // but rescheduling to a past date is not allowed.
  return baseSchema.superRefine((data, ctx) => {
    if (!initialStartAt) return;
    const dateChanged = data.startAt.getTime() !== initialStartAt.getTime();
    if (dateChanged && data.startAt <= new Date()) {
      ctx.addIssue({
        code: "custom",
        message: "date_must_be_future",
        path: ["startAt"],
      });
    }
  });
};

export type CreateActivityFormData = z.infer<typeof baseSchema>;
