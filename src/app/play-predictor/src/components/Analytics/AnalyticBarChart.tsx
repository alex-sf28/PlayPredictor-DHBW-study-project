import { Box, Heading } from '@chakra-ui/react';
import { BarChart, Legend, XAxis, YAxis, CartesianGrid, Tooltip, Bar } from 'recharts';

interface AnalyticBarChartProps {
    dataKey: string;
    name: string;
    description: string;
    chartData: {
        [key: string]: number | null;
    };
}
export default function AnalyticBarChart({ dataKey, name, description, chartData }: AnalyticBarChartProps) {

    const data = Object.entries(chartData).map(([key, value]) => ({ name: key, [dataKey]: value !== null ? Math.round(value * 10000) / 10000 : null }));
    return (

        <Box>
            <Heading size="md" mb={2}>{name}</Heading>
            <Heading size="sm" mb={4} color="gray.500">{description}</Heading>
            <BarChart
                style={{ width: '100%', maxWidth: '700px', maxHeight: '70vh', aspectRatio: 1.618 }}
                data={data}
                responsive>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis width="auto" />
                <Tooltip />
                <Legend />
                <Bar
                    dataKey={dataKey}
                    fill="var(--chakra-colors-brand-solid)"
                />
            </BarChart>
        </Box>

    )
}