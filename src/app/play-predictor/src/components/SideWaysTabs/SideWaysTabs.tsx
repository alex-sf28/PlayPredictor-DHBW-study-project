"use client"

import useActiveTab from "@/hooks/useActiveTab";
import { NavTabField, SideWaysTabsProps } from "@/types/navigation"
import { Box, For, Heading, LinkBox, Separator, Stack, Text } from "@chakra-ui/react"
import RouterLink from "../RouterLink/RouterLink";



export default function SideWaysTabs({ sideTabFields }: SideWaysTabsProps) {
    const isTabActive = useActiveTab()

    return (
        <Box width="12rem">
            {sideTabFields.map((section, index) => (
                <Box key={index}>
                    <Text fontSize='small' fontWeight='bold' color='gray'>{section.title}</Text>
                    <Stack>

                        {section.items.map((item) => (
                            <Box
                                layerStyle={isTabActive(item.pathName) ? "indicator.start" : undefined}
                                colorPalette={isTabActive(item.pathName) ? "brand" : "current"}
                                key={item.pathName}
                            >

                                <RouterLink path={item.pathName} marginX="2" >
                                    {item.displayName}
                                </RouterLink>
                            </Box>
                        ))}
                    </Stack>

                    {index < sideTabFields.length - 1 && <Separator my={2} size='sm' />}
                </Box>
            ))}
        </Box>
    );
}