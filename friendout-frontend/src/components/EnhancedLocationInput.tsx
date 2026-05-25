import { useEffect, useMemo, useState } from "react";
import { AlertCircle, ExternalLink, Link, MapPin, MessageCircle } from "lucide-react";

import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { LocalisationType as LocalisationTypeEnum } from "@/features/localisation/types/localisation.type";
import type { Localisation, LocalisationType } from "@/features/localisation/types/localisation.type";
import {
    extractLocationNameFromMapsUrl,
    generateGoogleMapsUrl,
    isGoogleMapsLink,
    isValidAddress,
    validateGoogleMapsUrl,
} from "@/lib/maps";
import { getTranslation } from "@/i18n";

import VirtualLocationInput from "./VirtualLocationInput";

interface EnhancedLocationInputProps {
    value: Localisation | null;
    onChange: (localisationData: Localisation | null) => void;
    required?: boolean;
    placeholder?: string;
}

const TAB_VALUES = {
    address: String(LocalisationTypeEnum.Address),
    mapsLink: String(LocalisationTypeEnum.MapLink),
    virtual: String(LocalisationTypeEnum.Virtual),
} as const;

type LocalisationTab = (typeof TAB_VALUES)[keyof typeof TAB_VALUES];

const tabFromType = (type: LocalisationType | undefined): LocalisationTab => {
    if (type === LocalisationTypeEnum.MapLink) return TAB_VALUES.mapsLink;
    if (type === LocalisationTypeEnum.Virtual) return TAB_VALUES.virtual;
    return TAB_VALUES.address;
};

const typeFromTab = (tab: LocalisationTab): LocalisationType => {
    if (tab === TAB_VALUES.mapsLink) return LocalisationTypeEnum.MapLink;
    if (tab === TAB_VALUES.virtual) return LocalisationTypeEnum.Virtual;
    return LocalisationTypeEnum.Address;
};

const getLocalisationFromAddress = (address: string): Localisation => {
    const hasValue = address.trim().length > 0;
    return {
        type: LocalisationTypeEnum.Address,
        address: hasValue ? address : undefined,
        displayName: hasValue ? address : undefined,
    };
};

const getLocalisationFromMapsLink = (mapsLink: string): Localisation => {
    const trimmedLink = mapsLink.trim();
    const locationName = trimmedLink ? extractLocationNameFromMapsUrl(trimmedLink) : "";
    return {
        type: LocalisationTypeEnum.MapLink,
        mapLink: trimmedLink || undefined,
        address: locationName || undefined,
        displayName: locationName || getTranslation("enhanced_location_input.virtual_display_name"),
    };
};

export default function EnhancedLocationInput({
    value,
    onChange,
    required = false,
    placeholder = getTranslation("enhanced_location_input.address_placeholder"),
}: EnhancedLocationInputProps) {
    const [activeTab, setActiveTab] = useState<LocalisationTab>(tabFromType(value?.type));
    const [addressValue, setAddressValue] = useState(value?.type === LocalisationTypeEnum.Address ? value.address || "" : "");
    const [mapsLinkValue, setMapsLinkValue] = useState(value?.type === LocalisationTypeEnum.MapLink ? value.mapLink || "" : "");
    const [mapsLinkError, setMapsLinkError] = useState("");

    useEffect(() => {
        if (!value) {
            setActiveTab(TAB_VALUES.address);
            setAddressValue("");
            setMapsLinkValue("");
            setMapsLinkError("");
            return;
        }

        setActiveTab(tabFromType(value.type));

        if (value.type === LocalisationTypeEnum.Address) {
            setAddressValue(value.address || "");
            setMapsLinkError("");
        }

        if (value.type === LocalisationTypeEnum.MapLink) {
            setMapsLinkValue(value.mapLink || "");
            if (value.mapLink?.trim()) {
                const validation = validateGoogleMapsUrl(value.mapLink);
                setMapsLinkError(validation.isValid ? "" : validation.error || "");
            } else {
                setMapsLinkError("");
            }
        }
    }, [value]);

    useEffect(() => {
        if (activeTab !== TAB_VALUES.address) return;
        if (!isGoogleMapsLink(addressValue)) return;

        setMapsLinkValue(addressValue);
        setActiveTab(TAB_VALUES.mapsLink);
        const validation = validateGoogleMapsUrl(addressValue);
        setMapsLinkError(validation.isValid ? "" : validation.error || "");
        onChange(getLocalisationFromMapsLink(addressValue));
    }, [activeTab, addressValue, onChange]);

    const showAddressMapButton = useMemo(() => isValidAddress(addressValue), [addressValue]);
    const canOpenMapsLink = mapsLinkValue.trim().length > 0 && mapsLinkError.length === 0;

    const handleTabChange = (nextTab: string) => {
        const newTab = nextTab as LocalisationTab;
        setActiveTab(newTab);

        if (newTab === TAB_VALUES.address) {
            onChange(getLocalisationFromAddress(addressValue));
            return;
        }

        if (newTab === TAB_VALUES.mapsLink) {
            if (mapsLinkValue.trim()) {
                const validation = validateGoogleMapsUrl(mapsLinkValue);
                setMapsLinkError(validation.isValid ? "" : validation.error || "");
            } else {
                setMapsLinkError("");
            }
            onChange(getLocalisationFromMapsLink(mapsLinkValue));
            return;
        }

        onChange({
            type: typeFromTab(TAB_VALUES.virtual),
            displayName: getTranslation("enhanced_location_input.virtual_display_name"),
        });
    };

    const handleAddressChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const nextValue = event.target.value;
        setAddressValue(nextValue);
        onChange(getLocalisationFromAddress(nextValue));
    };

    const handleMapsLinkChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const nextValue = event.target.value;
        setMapsLinkValue(nextValue);

        if (nextValue.trim()) {
            const validation = validateGoogleMapsUrl(nextValue);
            setMapsLinkError(validation.isValid ? "" : validation.error || "");
        } else {
            setMapsLinkError("");
        }

        onChange(getLocalisationFromMapsLink(nextValue));
    };

    return (
        <div className="space-y-3">
            <Label>
                {getTranslation("enhanced_location_input.label")} {required && "*"}
            </Label>

            <Tabs value={activeTab} onValueChange={handleTabChange}>
                <TabsList className="grid w-full grid-cols-3">
                    <TabsTrigger value={TAB_VALUES.address} className="flex items-center gap-2 cursor-pointer hover:bg-blue-100">
                        <MapPin className="w-4 h-4" />
                        <span className="hidden sm:inline">{getTranslation("enhanced_location_input.tab_address")}</span>
                    </TabsTrigger>
                    <TabsTrigger value={TAB_VALUES.mapsLink} className="flex items-center gap-2 cursor-pointer hover:bg-blue-100">
                        <Link className="w-4 h-4" />
                        <span className="hidden sm:inline">{getTranslation("enhanced_location_input.tab_maps_link")}</span>
                    </TabsTrigger>
                    <TabsTrigger value={TAB_VALUES.virtual} className="flex items-center gap-2 cursor-pointer hover:bg-blue-100">
                        <MessageCircle className="w-4 h-4" />
                        <span className="hidden sm:inline">{getTranslation("enhanced_location_input.tab_virtual")}</span>
                    </TabsTrigger>
                </TabsList>

                <TabsContent value={TAB_VALUES.address} className="space-y-2">
                    <div className="flex items-center gap-2">
                        <Input
                            value={addressValue}
                            onChange={handleAddressChange}
                            placeholder={placeholder}
                            required={required && activeTab === TAB_VALUES.address}
                            className="flex-1"
                        />
                        {showAddressMapButton && (
                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={() => window.open(generateGoogleMapsUrl(addressValue), "_blank")}
                                className="flex items-center gap-1 px-2 sm:px-3"
                                title={getTranslation("enhanced_location_input.address_open_maps_title")}
                            >
                                <MapPin className="w-4 h-4" />
                                <ExternalLink className="hidden sm:inline w-3 h-3" />
                            </Button>
                        )}
                    </div>
                </TabsContent>

                <TabsContent value={TAB_VALUES.mapsLink} className="space-y-2">
                    <div className="flex gap-2">
                        <Input
                            value={mapsLinkValue}
                            onChange={handleMapsLinkChange}
                            placeholder={getTranslation("enhanced_location_input.maps_link_placeholder")}
                            required={required && activeTab === TAB_VALUES.mapsLink}
                            className="flex-1"
                        />
                        {canOpenMapsLink && (
                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={() => window.open(mapsLinkValue, "_blank")}
                                className="flex items-center gap-1 px-2 sm:px-3"
                                title={getTranslation("enhanced_location_input.maps_link_test_title")}
                            >
                                <ExternalLink className="w-4 h-4" />
                                <span className="hidden sm:inline">{getTranslation("enhanced_location_input.maps_link_test_button")}</span>
                            </Button>
                        )}
                    </div>

                    {mapsLinkError && (
                        <Alert variant="destructive">
                            <AlertCircle className="h-4 w-4" />
                            <AlertDescription>{mapsLinkError}</AlertDescription>
                        </Alert>
                    )}

                    {canOpenMapsLink && (
                        <div className="text-xs text-muted-foreground">
                            <p>{getTranslation("enhanced_location_input.maps_link_valid")}</p>
                            <p>
                                {getTranslation("enhanced_location_input.maps_link_detected_place")}
                                <span className="font-medium">{extractLocationNameFromMapsUrl(mapsLinkValue)}</span>
                            </p>
                        </div>
                    )}

                </TabsContent>

                <TabsContent value={TAB_VALUES.virtual} className="mt-4">
                    <VirtualLocationInput value={value?.type === LocalisationTypeEnum.Virtual ? value : null} onChange={onChange} />
                </TabsContent>
            </Tabs>
        </div>
    );
}
