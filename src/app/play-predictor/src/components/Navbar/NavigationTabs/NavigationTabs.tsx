"use client"

import RouterLink from "@/components/RouterLink/RouterLink";
import useActiveTab from "@/hooks/useActiveTab";
import { NavTabProps } from "@/types/navigation";
import { Box, Text } from "@chakra-ui/react";
import Link from "next/link";


export default function NavigationTabs({ navBarFields }: NavTabProps) {
    const isTabActive = useActiveTab()

    return (
        <Box display="flex" flexDirection="row" >
            {navBarFields.map((field) => {
                return (
                    <Box
                        key={field.pathName}
                        padding="0.3rem"
                        display="flex"
                        flexDirection="row"
                        alignItems="center"
                        layerStyle={isTabActive(field.pathName) ? "indicator.bottom" : undefined}
                        colorPalette={isTabActive(field.pathName) ? "brand" : undefined}
                    >

                        <RouterLink
                            path={`/${field.pathName}`}
                            display="flex"
                            flexDirection="row"
                            alignItems="center"
                            gapX="0.3rem"
                            color={isTabActive(field.pathName) ? "primary" : "current"}
                            fontWeight="bold"
                        >
                            {field.displayName}
                        </RouterLink>
                    </Box>
                )
            })}
        </Box>
    );
}