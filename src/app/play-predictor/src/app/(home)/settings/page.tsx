"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { sideWaysTabFields } from "@/config/sideWaysTabsFields";
import SettingsSkeleton from "@/components/Skeletons/SettingsSkeleton";

export default function SettingsPage() {
    const router = useRouter();

    useEffect(() => {
        const first = sideWaysTabFields[0].items[0].pathName;
        router.replace(first);
    }, [router]);

    return (
        <SettingsSkeleton />
    );
}
