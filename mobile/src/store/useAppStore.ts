import {create} from 'zustand';
type State={selectedUri:string|null; setSelectedUri:(uri:string|null)=>void}; export const useAppStore=create<State>(set=>({selectedUri:null,setSelectedUri:selectedUri=>set({selectedUri})}));
