import React, {useState} from 'react';
import {Text, View} from 'react-native';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query';
import {HomeScreen} from './src/screens/HomeScreen';
import {PreviewScreen} from './src/screens/PreviewScreen';
import {ResultScreen} from './src/screens/ResultScreen';
import {HistoryScreen} from './src/screens/HistoryScreen';
import {Button, styles} from './src/components/UI';

const queryClient = new QueryClient();

function ErrorScreen({navigation}: any) {
  return <View style={{flex: 1, backgroundColor: '#10101D', padding: 24, justifyContent: 'center'}}>
    <Text style={{color: '#fff', fontSize: 28, fontWeight: '800'}}>Something went wrong</Text>
    <Text style={styles.muted}>Check that the backend is running, then try again.</Text>
    <Button title="Try again" onPress={() => navigation.goBack()} />
  </View>;
}

export default function App() {
  const [route, setRoute] = useState('Home');
  const [params, setParams] = useState<any>({});
  const navigation = {
    navigate: (name: string, nextParams?: any) => { setParams(nextParams || {}); setRoute(name); },
    replace: (name: string, nextParams?: any) => { setParams(nextParams || {}); setRoute(name); },
    goBack: () => setRoute('Home'),
    popToTop: () => { setParams({}); setRoute('Home'); },
  };
  const screens: any = {Home: HomeScreen, Preview: PreviewScreen, Result: ResultScreen, History: HistoryScreen, Error: ErrorScreen};
  const Screen = screens[route] || HomeScreen;
  return <QueryClientProvider client={queryClient}><Screen navigation={navigation} route={{params}} /></QueryClientProvider>;
}
