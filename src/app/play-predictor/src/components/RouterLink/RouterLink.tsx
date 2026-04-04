import { LinkProps } from "@chakra-ui/react";
import { Link as ChakraLink } from "@chakra-ui/react"
import NextLink from "next/link"

export interface RouterLinkProps extends LinkProps {
    path: string
}

export default function RouterLink({ path, children, ...rest }: RouterLinkProps) {

    return (
        <ChakraLink
            {...rest} asChild>
            <NextLink href={path}>
                {children}
            </NextLink>
        </ChakraLink>
    )
}