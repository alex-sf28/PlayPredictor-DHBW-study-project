import { Box, Heading, Separator } from "@chakra-ui/react"

export type sectionProps = {
    children: React.ReactNode
    heading: string
    highlight?: boolean
}

export default function Section({ children, heading, highlight }: sectionProps) {

    return (<Box marginBottom='2rem' >
        <Heading fontWeight={highlight ? 'bold' : ''} color={highlight ? 'danger' : 'current'}>{heading}</Heading>
        <Separator marginBottom='0.8rem' />
        <Box gapY='0.6rem' direction='column' >
            {children}
        </Box>

    </Box>)
}