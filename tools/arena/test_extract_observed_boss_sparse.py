from copy import deepcopy
import unittest
import extract_observed_boss_sparse as e

class BossSparseTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'bspar2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_positive_controls_and_natural_helper_death(self):
        result=e.extract()['records']
        self.assertEqual(len(result),6)
        self.assertEqual(result[3]['summary']['final_speed'],250)
        self.assertLess(result[5]['death'][0]['time']-result[5]['summon'][0]['time'],.011)

    def test_partial_wrong_spell_event_enum_and_identity_rejected(self):
        for row,key,value in [('meta','complete',0),('cripple_spell2','kind',0),('doom_event2','source_id',0)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_no_zero_default_without_positive_buff_and_actual_attack(self):
        for row,key,value in [('aura_on','warm_B07R',0),('cripple_event4','damage',1),('doom','after_move_B0BN',0)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_doom_life_and_hidden_helper_death_are_separate_from_events(self):
        for row,key,value in [('doom','sample25_hp',846),('inferno_helper_death0','hidden',0),
                              ('inferno_helper_death0','time',2.6),('cripple_event4','damage',float('nan'))]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

if __name__=='__main__':unittest.main()
