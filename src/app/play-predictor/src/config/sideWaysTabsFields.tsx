import { SideWaysTabsSection } from "@/types/navigation";

export const sideWaysTabFields: SideWaysTabsSection[] = [
    {
        title: "General",
        items: [
            {
                displayName: "Account",
                pathName: "/settings/account"
            }
        ]
    },
    {
        title: "Access",
        items: [
            {
                displayName: "Authentication",
                pathName: "/settings/authentication"
            },
            {
                displayName: "Connected Services",
                pathName: "/settings/connected-services"
            }
        ]
    }
]